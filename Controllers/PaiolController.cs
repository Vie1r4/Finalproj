using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Tutorial Class 8: sem sessão iniciada o utilizador não pode aceder – [Authorize] em todo o controller.
    /// </summary>
    [Authorize]
    public class PaiolController : Controller
    {
        private readonly FinalprojContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PaiolController(FinalprojContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>Ids dos paióis a que o utilizador atual tem acesso (por cargo).</summary>
        private async Task<List<int>> ObterPaiolIdsComAcessoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = user == null ? Array.Empty<string>() : (await _userManager.GetRolesAsync(user)).ToArray();
            return await _context.PaiolAcessos
                .Where(a => roles.Contains(a.RoleName))
                .Select(a => a.PaiolId)
                .Distinct()
                .ToListAsync();
        }

        // GET: Paiol — página operacional: lista de paióis com acesso (Detalhes, Adicionar, Retirar)
        public async Task<IActionResult> Index()
        {
            var idsAcesso = await ObterPaiolIdsComAcessoAsync();
            var lista = await _context.Paiol
                .Where(p => idsAcesso.Contains(p.Id))
                .OrderBy(p => p.Nome)
                .ToListAsync();
            return View(lista);
        }

        // GET: Paiol/Movimentos — ver entradas ou saídas, com filtro por paiol (Class 8: só paióis com acesso)
        public async Task<IActionResult> Movimentos(string? tipo, int? paiolId)
        {
            var idsAcesso = await ObterPaiolIdsComAcessoAsync();
            var paióis = await _context.Paiol
                .Where(p => idsAcesso.Contains(p.Id))
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewData["PaiolId"] = new SelectList(paióis, "Id", "Nome", paiolId);
            ViewData["Tipo"] = tipo ?? "";

            if (string.IsNullOrEmpty(tipo))
            {
                ViewData["Entradas"] = new List<EntradaPaiol>();
                ViewData["Saidas"] = new List<SaidaPaiol>();
                return View();
            }

            if (tipo == "Entradas")
            {
                var query = _context.EntradasPaiol
                    .Include(e => e.Paiol)
                    .Include(e => e.Produto)
                    .Where(e => idsAcesso.Contains(e.PaiolId));
                if (paiolId.HasValue)
                    query = query.Where(e => e.PaiolId == paiolId.Value);
                var entradas = await query.OrderByDescending(e => e.DataEntrada).ToListAsync();
                ViewData["Entradas"] = entradas;
                ViewData["Saidas"] = new List<SaidaPaiol>();
            }
            else
            {
                var query = _context.SaidasPaiol
                    .Include(s => s.Paiol)
                    .Include(s => s.Produto)
                    .Where(s => idsAcesso.Contains(s.PaiolId));
                if (paiolId.HasValue)
                    query = query.Where(s => s.PaiolId == paiolId.Value);
                var saidas = await query.OrderByDescending(s => s.DataSaida).ToListAsync();
                ViewData["Entradas"] = new List<EntradaPaiol>();
                ViewData["Saidas"] = saidas;
            }

            return View();
        }

        // GET: Paiol/Gestao — CRUD completo; apenas Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Gestao()
        {
            return View(await _context.Paiol.OrderBy(p => p.Nome).ToListAsync());
        }

        // GET: Paiol/Conteudo/5 — conteúdo do paiol (itens em stock); retirar por item (Class 4: navegação)
        public async Task<IActionResult> Conteudo(int? id)
        {
            if (id == null)
                return NotFound();

            var idsAcesso = await ObterPaiolIdsComAcessoAsync();
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !idsAcesso.Contains(id.Value))
                return Forbid();

            var paiol = await _context.Paiol.FirstOrDefaultAsync(m => m.Id == id);
            if (paiol == null)
                return NotFound();

            var entradas = await _context.EntradasPaiol.Where(e => e.PaiolId == id).Include(e => e.Produto).ToListAsync();
            var saidas = await _context.SaidasPaiol.Where(s => s.PaiolId == id).ToListAsync();
            var stockPorProduto = entradas
                .GroupBy(e => e.ProdutoId)
                .Select(g => new { ProdutoId = g.Key, Entradas = g.Sum(e => e.Quantidade) })
                .ToDictionary(x => x.ProdutoId, x => x.Entradas);
            foreach (var s in saidas)
            {
                if (stockPorProduto.ContainsKey(s.ProdutoId))
                    stockPorProduto[s.ProdutoId] -= s.Quantidade;
            }
            var produtosIds = stockPorProduto.Keys.ToList();
            var produtos = await _context.Produtos.Where(pr => produtosIds.Contains(pr.Id)).ToDictionaryAsync(pr => pr.Id);
            var carga = stockPorProduto
                .Where(kv => kv.Value > 0)
                .Select(kv =>
                {
                    var p = produtos.GetValueOrDefault(kv.Key);
                    return p == null ? null : new CargaPaiolItem { ProdutoId = p.Id, ProdutoNome = p.Nome, Quantidade = kv.Value, NEMPorUnidade = p.NEMPorUnidade };
                })
                .Where(x => x != null)
                .Cast<CargaPaiolItem>()
                .ToList();

            ViewData["Carga"] = carga;
            return View(paiol);
        }

        // GET: Paiol/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var idsAcesso = await ObterPaiolIdsComAcessoAsync();
            var isAdmin = User.IsInRole("Admin");
            if (!isAdmin && !idsAcesso.Contains(id.Value))
                return Forbid();

            var paiol = await _context.Paiol.FirstOrDefaultAsync(m => m.Id == id);
            if (paiol == null)
                return NotFound();

            var entradas = await _context.EntradasPaiol.Where(e => e.PaiolId == id).Include(e => e.Produto).ToListAsync();
            var saidas = await _context.SaidasPaiol.Where(s => s.PaiolId == id).Include(s => s.Produto).ToListAsync();
            var stockPorProduto = entradas
                .GroupBy(e => e.ProdutoId)
                .Select(g => new { ProdutoId = g.Key, Entradas = g.Sum(e => e.Quantidade) })
                .ToDictionary(x => x.ProdutoId, x => x.Entradas);
            foreach (var s in saidas)
            {
                if (stockPorProduto.ContainsKey(s.ProdutoId))
                    stockPorProduto[s.ProdutoId] -= s.Quantidade;
            }
            var produtosIds = stockPorProduto.Keys.ToList();
            var produtos = await _context.Produtos.Where(pr => produtosIds.Contains(pr.Id)).ToDictionaryAsync(pr => pr.Id);
            var carga = stockPorProduto
                .Where(kv => kv.Value > 0)
                .Select(kv =>
                {
                    var p = produtos.GetValueOrDefault(kv.Key);
                    return p == null ? null : new CargaPaiolItem { ProdutoId = p.Id, ProdutoNome = p.Nome, Quantidade = kv.Value, NEMPorUnidade = p.NEMPorUnidade };
                })
                .Where(x => x != null)
                .Cast<CargaPaiolItem>()
                .ToList();
            var nemAtual = carga.Sum(x => x.NEMTotal);
            var cargosAcesso = await _context.PaiolAcessos.Where(a => a.PaiolId == id).Select(a => a.RoleName).ToListAsync();
            ViewData["NEMAtual"] = nemAtual;
            ViewData["Carga"] = carga;
            ViewData["CargosAcesso"] = cargosAcesso;
            return View(paiol);
        }

        // GET: Paiol/Create (apenas Admin – Gestão de Paióis)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["PerfisRisco"] = new SelectList(ConstantesPaiol.LicencasParaDropdown(), "Value", "Text");
            ViewData["Estados"] = ConstantesPaiol.Estados;
            ViewData["CargosDisponiveis"] = ConstantesPaiol.CargosDisponiveis;
            return View();
        }

        // POST: Paiol/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nome,Localizacao,LimiteMLE,PerfilRisco,Estado")] Paiol paiol, string[]? CargosAcesso)
        {
            if (ModelState.IsValid)
            {
                if (!ConstantesPaiol.PerfisRisco.Contains(paiol.PerfilRisco) || !ConstantesPaiol.Estados.Contains(paiol.Estado))
                {
                    ModelState.AddModelError(string.Empty, "Perfil de risco ou estado inválido.");
                    ViewData["PerfisRisco"] = new SelectList(ConstantesPaiol.LicencasParaDropdown(), "Value", "Text", paiol.PerfilRisco);
                    ViewData["Estados"] = ConstantesPaiol.Estados;
                    ViewData["CargosDisponiveis"] = ConstantesPaiol.CargosDisponiveis;
                    return View(paiol);
                }
                _context.Add(paiol);
                await _context.SaveChangesAsync();
                if (CargosAcesso != null)
                {
                    foreach (var role in CargosAcesso)
                    {
                        if (ConstantesPaiol.CargosDisponiveis.Contains(role))
                            _context.PaiolAcessos.Add(new PaiolAcesso { PaiolId = paiol.Id, RoleName = role });
                    }
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction(nameof(Gestao));
            }
            ViewData["PerfisRisco"] = new SelectList(ConstantesPaiol.LicencasParaDropdown(), "Value", "Text", paiol.PerfilRisco);
            ViewData["Estados"] = ConstantesPaiol.Estados;
            ViewData["CargosDisponiveis"] = ConstantesPaiol.CargosDisponiveis;
            return View(paiol);
        }

        // GET: Paiol/Edit/5 (apenas Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var paiol = await _context.Paiol.FindAsync(id);
            if (paiol == null)
                return NotFound();

            var acessos = await _context.PaiolAcessos.Where(a => a.PaiolId == id).Select(a => a.RoleName).ToListAsync();
            ViewData["PerfisRisco"] = new SelectList(ConstantesPaiol.LicencasParaDropdown(), "Value", "Text", paiol.PerfilRisco);
            ViewData["Estados"] = ConstantesPaiol.Estados;
            ViewData["CargosDisponiveis"] = ConstantesPaiol.CargosDisponiveis;
            ViewData["CargosSelecionados"] = acessos;
            return View(paiol);
        }

        // POST: Paiol/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Localizacao,LimiteMLE,PerfilRisco,Estado")] Paiol paiol, string[]? CargosAcesso)
        {
            if (id != paiol.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paiol);
                    await _context.SaveChangesAsync();

                    var existentes = await _context.PaiolAcessos.Where(a => a.PaiolId == id).ToListAsync();
                    _context.PaiolAcessos.RemoveRange(existentes);
                    if (CargosAcesso != null)
                    {
                        foreach (var role in CargosAcesso)
                        {
                            if (ConstantesPaiol.CargosDisponiveis.Contains(role))
                                _context.PaiolAcessos.Add(new PaiolAcesso { PaiolId = id, RoleName = role });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PaiolExistsAsync(paiol.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Gestao));
            }
            var acessos = await _context.PaiolAcessos.Where(a => a.PaiolId == id).Select(a => a.RoleName).ToListAsync();
            ViewData["PerfisRisco"] = new SelectList(ConstantesPaiol.LicencasParaDropdown(), "Value", "Text", paiol.PerfilRisco);
            ViewData["Estados"] = ConstantesPaiol.Estados;
            ViewData["CargosDisponiveis"] = ConstantesPaiol.CargosDisponiveis;
            ViewData["CargosSelecionados"] = acessos;
            return View(paiol);
        }

        // GET: Paiol/Delete/5 (apenas Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var paiol = await _context.Paiol.FirstOrDefaultAsync(m => m.Id == id);
            if (paiol == null)
                return NotFound();

            return View(paiol);
        }

        // POST: Paiol/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paiol = await _context.Paiol.FindAsync(id);
            if (paiol != null)
            {
                _context.Paiol.Remove(paiol);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Gestao));
        }

        private async Task<bool> PaiolExistsAsync(int id)
        {
            return await _context.Paiol.AnyAsync(e => e.Id == id);
        }
    }
}
