using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Processo de decisão de entrada no paiol (Class 3 + 4). Class 8: só aparecem paióis a que o cargo do utilizador tem acesso.
    /// </summary>
    [Authorize]
    public class EntradaPaiolController : Controller
    {
        private readonly FinalprojContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Services.ILogSistemaService _logSistema;

        public EntradaPaiolController(FinalprojContext context, UserManager<IdentityUser> userManager, Services.ILogSistemaService logSistema)
        {
            _context = context;
            _userManager = userManager;
            _logSistema = logSistema;
        }

        /// <summary> O histórico de entradas só é acessível através de Paiol/Movimentos. </summary>
        public IActionResult Index()
        {
            return RedirectToAction("Movimentos", "Paiol", new { tipo = "Entradas" });
        }

        /// <summary> GET: formulário "Registar entrada". Filtros: paiolId, classificacao, grupoCompatibilidade, filtroTecnico, calibre. </summary>
        public async Task<IActionResult> Registar(int? paiolId, string? classificacao, string? grupoCompatibilidade, string? filtroTecnico, string? calibre)
        {
            var model = new EntradaPaiolViewModel();
            if (paiolId.HasValue)
                model.PaiolId = paiolId.Value;
            ViewData["Classificacao"] = classificacao ?? "";
            ViewData["GrupoCompatibilidade"] = grupoCompatibilidade ?? "";
            ViewData["FiltroTecnico"] = filtroTecnico ?? "";
            ViewData["Calibre"] = calibre ?? "";
            await PopularDropdownsAsync(paiolId, null, classificacao, grupoCompatibilidade, filtroTecnico, calibre);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar(EntradaPaiolViewModel model)
        {
            var paiol = await _context.Paiol.FindAsync(model.PaiolId);
            var produto = await _context.Produtos.FindAsync(model.ProdutoId);

            if (paiol == null || produto == null)
            {
                ModelState.AddModelError(string.Empty, "Paiol ou produto inválido.");
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null, null);
                return View(model);
            }

            if (paiol.Estado != ConstantesPaiol.EstadoAtivo)
            {
                ModelState.AddModelError(string.Empty, "O paiol está em manutenção e não pode receber carga.");
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null, null);
                return View(model);
            }

            // Passo C (1) e restantes regras: Motor de Validação (PROMPT_Motor_Validacao_Paiol)
            var stockPorProduto = new Dictionary<int, decimal>();
            var entradasNoPaiol = await _context.EntradasPaiol
                .Where(e => e.PaiolId == paiol.Id)
                .Include(e => e.Produto)
                .ToListAsync();
            var saidasNoPaiol = await _context.SaidasPaiol
                .Where(s => s.PaiolId == paiol.Id)
                .ToListAsync();
            foreach (var e in entradasNoPaiol)
                stockPorProduto[e.ProdutoId] = stockPorProduto.GetValueOrDefault(e.ProdutoId) + e.Quantidade;
            foreach (var s in saidasNoPaiol)
            {
                if (stockPorProduto.ContainsKey(s.ProdutoId))
                    stockPorProduto[s.ProdutoId] -= s.Quantidade;
            }

            var produtosNoPaiol = new List<ProdutoNoPaiolDto>();
            var mleAtualPaiol = 0m;
            foreach (var kv in stockPorProduto.Where(kv => kv.Value > 0))
            {
                var p = entradasNoPaiol.First(e => e.ProdutoId == kv.Key).Produto;
                var grupo = (p.GrupoCompatibilidade ?? "G").Trim().ToUpperInvariant();
                if (string.IsNullOrEmpty(grupo)) grupo = "G";
                produtosNoPaiol.Add(new ProdutoNoPaiolDto
                {
                    Divisao = p.FamiliaRisco ?? "",
                    Grupo = grupo,
                    Quantidade = kv.Value,
                    NEMPorUnidade = p.NEMPorUnidade
                });
                mleAtualPaiol += kv.Value * p.NEMPorUnidade;
            }

            var resultado = MotorValidacaoPaiol.ValidarEntrada(
                produto, paiol, model.Quantidade, produtosNoPaiol, mleAtualPaiol, model.DataValidade);

            if (!resultado.Aprovado)
            {
                foreach (var err in resultado.Erros)
                    ModelState.AddModelError(string.Empty, $"[{err.Codigo}] {err.Mensagem}");
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null, null);
                return View(model);
            }

            // Gravar entrada: ocupação em NEM (soma direta, sem fatores por divisão)
            var mleEntrada = model.Quantidade * produto.NEMPorUnidade;
            var mleTotalApos = mleAtualPaiol + mleEntrada;

            _context.EntradasPaiol.Add(new EntradaPaiol
            {
                PaiolId = paiol.Id,
                ProdutoId = produto.Id,
                Quantidade = model.Quantidade,
                DataEntrada = DateTime.UtcNow,
                FuncionarioRegistouUserId = _userManager.GetUserId(User),
                NumeroLote = string.IsNullOrWhiteSpace(model.NumeroLote) ? null : model.NumeroLote.Trim(),
                DataFabrico = model.DataFabrico,
                DataValidade = model.DataValidade
            });

            if (!string.IsNullOrEmpty(resultado.DivisaoDominanteResultante))
            {
                paiol.DivisaoDominante = resultado.DivisaoDominanteResultante;
            }

            await _context.SaveChangesAsync();

            var user = await _userManager.GetUserAsync(User);
            await _logSistema.RegistarAsync("ENTRADA_STOCK", user?.Id, user?.UserName, new
            {
                produto_id = produto.Id,
                produto_nome = produto.Nome,
                numero_lote = model.NumeroLote,
                quantidade_kg = model.Quantidade,
                paiol_id = paiol.Id,
                paiol_nome = paiol.Nome,
                mle_total_paiol_apos = mleTotalApos
            });

            if (resultado.Avisos.Count > 0)
                TempData["AvisosValidacao"] = string.Join(" | ", resultado.Avisos.Select(a => a.Mensagem));
            TempData["EntradaSucesso"] = $"Entrada registada: {model.Quantidade} × {produto.Nome} no paiol {paiol.Nome}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopularDropdownsAsync(int? paiolId, int? produtoId, string? classificacao, string? grupoCompatibilidade, string? filtroTecnico, string? calibre)
        {
            var user = await _userManager.GetUserAsync(User);
            var rolesDoUtilizador = user == null ? Array.Empty<string>() : (await _userManager.GetRolesAsync(user)).ToArray();

            var idsPaióisComAcesso = await _context.PaiolAcessos
                .Where(a => rolesDoUtilizador.Contains(a.RoleName))
                .Select(a => a.PaiolId)
                .Distinct()
                .ToListAsync();

            var paióisComAcesso = await _context.Paiol
                .Where(p => p.Estado == ConstantesPaiol.EstadoAtivo && idsPaióisComAcesso.Contains(p.Id))
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewData["PaiolId"] = new SelectList(paióisComAcesso, "Id", "Nome", paiolId);

            var query = _context.Produtos.AsQueryable();
            if (!string.IsNullOrEmpty(classificacao))
                query = query.Where(p => p.FamiliaRisco == classificacao);
            if (!string.IsNullOrEmpty(grupoCompatibilidade))
                query = query.Where(p => p.GrupoCompatibilidade == grupoCompatibilidade);
            if (!string.IsNullOrEmpty(filtroTecnico))
                query = query.Where(p => p.FiltroTecnico == filtroTecnico);
            if (!string.IsNullOrEmpty(calibre))
                query = query.Where(p => p.Calibre == calibre);
            var produtos = await query.OrderBy(p => p.Nome).ToListAsync();
            ViewData["ProdutoId"] = new SelectList(produtos, "Id", "Nome", produtoId);
        }
    }
}
