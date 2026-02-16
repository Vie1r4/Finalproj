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

        public EntradaPaiolController(FinalprojContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary> O histórico de entradas só é acessível através de Paiol/Movimentos. </summary>
        public IActionResult Index()
        {
            return RedirectToAction("Movimentos", "Paiol", new { tipo = "Entradas" });
        }

        /// <summary> GET: formulário "Registar entrada". Opcional: paiolId, classificacao, filtroTecnico, calibre para filtrar produtos (subdivisão do catálogo). </summary>
        public async Task<IActionResult> Registar(int? paiolId, string? classificacao, string? filtroTecnico, string? calibre)
        {
            var model = new EntradaPaiolViewModel();
            if (paiolId.HasValue)
                model.PaiolId = paiolId.Value;
            ViewData["Classificacao"] = classificacao ?? "";
            ViewData["FiltroTecnico"] = filtroTecnico ?? "";
            ViewData["Calibre"] = calibre ?? "";
            await PopularDropdownsAsync(paiolId, null, classificacao, filtroTecnico, calibre);
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
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null);
                return View(model);
            }

            // Passo C (1): Compatibilidade licença–classificação de risco (Class 3: regras de negócio)
            if (!RegrasLicencaPaiol.ProdutoPodeEntrar(paiol.PerfilRisco, produto.FamiliaRisco))
            {
                ModelState.AddModelError(string.Empty,
                    RegrasLicencaPaiol.MensagemRecusa(paiol.PerfilRisco, produto.FamiliaRisco));
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null);
                return View(model);
            }

            // Paiol em manutenção – bloqueado
            if (paiol.Estado != ConstantesPaiol.EstadoAtivo)
            {
                ModelState.AddModelError(string.Empty,
                    "O paiol está em manutenção e não pode receber carga.");
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null);
                return View(model);
            }

            // Passo A: NEM da carga a entrar
            var nemEntrada = model.Quantidade * produto.NEMPorUnidade;

            // Passo B: NEM já dentro do paiol = stock efetivo (entradas − saídas) por produto
            var entradasNoPaiol = await _context.EntradasPaiol
                .Where(e => e.PaiolId == paiol.Id)
                .Include(e => e.Produto)
                .ToListAsync();
            var saidasNoPaiol = await _context.SaidasPaiol
                .Where(s => s.PaiolId == paiol.Id)
                .ToListAsync();
            var stockPorProduto = entradasNoPaiol.GroupBy(e => e.ProdutoId)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Quantidade));
            foreach (var s in saidasNoPaiol)
            {
                if (stockPorProduto.ContainsKey(s.ProdutoId))
                    stockPorProduto[s.ProdutoId] -= s.Quantidade;
            }
            var nemAtual = 0m;
            foreach (var kv in stockPorProduto.Where(kv => kv.Value > 0))
            {
                var prod = entradasNoPaiol.First(e => e.ProdutoId == kv.Key).Produto;
                nemAtual += kv.Value * prod.NEMPorUnidade;
            }

            // Passo C (2): Veredito – teto de segurança
            var totalPrevisao = nemAtual + nemEntrada;
            if (totalPrevisao > paiol.LimiteMLE)
            {
                var excesso = totalPrevisao - paiol.LimiteMLE;
                ModelState.AddModelError(string.Empty,
                    "Impossível. A entrada desta carga violaria o limite de segurança em " + excesso.ToString("N2") + " kg.");
                await PopularDropdownsAsync(model.PaiolId, model.ProdutoId, null, null, null);
                return View(model);
            }

            // Passo D: Aceitar – gravar entrada
            _context.EntradasPaiol.Add(new EntradaPaiol
            {
                PaiolId = paiol.Id,
                ProdutoId = produto.Id,
                Quantidade = model.Quantidade,
                DataEntrada = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["EntradaSucesso"] = $"Entrada registada: {model.Quantidade} × {produto.Nome} no paiol {paiol.Nome}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopularDropdownsAsync(int? paiolId, int? produtoId, string? classificacao, string? filtroTecnico, string? calibre)
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
            if (!string.IsNullOrEmpty(filtroTecnico))
                query = query.Where(p => p.FiltroTecnico == filtroTecnico);
            if (!string.IsNullOrEmpty(calibre))
                query = query.Where(p => p.Calibre == calibre);
            var produtos = await query.OrderBy(p => p.Nome).ToListAsync();
            ViewData["ProdutoId"] = new SelectList(produtos, "Id", "Nome", produtoId);
        }
    }
}
