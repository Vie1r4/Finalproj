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
        private readonly IWebHostEnvironment _env;
        private const string PastaDocumentosPaiol = "Documentos/Paiol";
        private static readonly string[] ExtensoesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };

        public PaiolController(FinalprojContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
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

        /// <summary>Calcula MLE atual e percentagem de ocupação por paiol (para listagens).</summary>
        private async Task<Dictionary<int, (decimal MleAtual, decimal PercentagemOcupacao)>> CalcularOcupacaoPorPaiolAsync(IEnumerable<Paiol> paióis)
        {
            var ids = paióis.Select(p => p.Id).ToList();
            if (ids.Count == 0)
                return new Dictionary<int, (decimal, decimal)>();

            var limitePorPaiol = paióis.ToDictionary(p => p.Id, p => p.LimiteMLE);
            var entradas = await _context.EntradasPaiol
                .Where(e => ids.Contains(e.PaiolId))
                .Include(e => e.Produto)
                .ToListAsync();
            var saidas = await _context.SaidasPaiol
                .Where(s => ids.Contains(s.PaiolId))
                .ToListAsync();

            var result = new Dictionary<int, (decimal, decimal)>();
            foreach (var id in ids)
            {
                var entradasPaiol = entradas.Where(e => e.PaiolId == id).ToList();
                var saidasPaiol = saidas.Where(s => s.PaiolId == id).ToList();
                var stockPorProduto = entradasPaiol
                    .GroupBy(e => e.ProdutoId)
                    .ToDictionary(g => g.Key, g => g.Sum(e => e.Quantidade));
                foreach (var s in saidasPaiol)
                {
                    if (stockPorProduto.ContainsKey(s.ProdutoId))
                        stockPorProduto[s.ProdutoId] -= s.Quantidade;
                }
                decimal mleAtual = 0;
                foreach (var kv in stockPorProduto.Where(kv => kv.Value > 0))
                {
                    var prod = entradasPaiol.First(e => e.ProdutoId == kv.Key).Produto;
                    mleAtual += kv.Value * prod.NEMPorUnidade;
                }
                var limite = limitePorPaiol[id];
                var percentagem = limite > 0 ? mleAtual / limite * 100 : 0;
                result[id] = (mleAtual, percentagem);
            }
            return result;
        }

        // GET: Paiol — página operacional: lista de paióis com acesso (Detalhes, Adicionar, Retirar)
        public async Task<IActionResult> Index()
        {
            var idsAcesso = await ObterPaiolIdsComAcessoAsync();
            var lista = await _context.Paiol
                .Where(p => idsAcesso.Contains(p.Id))
                .OrderBy(p => p.Nome)
                .ToListAsync();
            var ocupacao = await CalcularOcupacaoPorPaiolAsync(lista);
            var viewModel = lista.Select(p => new PaiolComOcupacaoViewModel
            {
                Paiol = p,
                MleAtual = ocupacao.GetValueOrDefault(p.Id).MleAtual,
                PercentagemOcupacao = ocupacao.GetValueOrDefault(p.Id).PercentagemOcupacao
            }).ToList();
            return View(viewModel);
        }

        /// <summary>Stock físico (entradas - saídas) por produto, apenas nos paióis com acesso.</summary>
        private async Task<Dictionary<int, decimal>> ObterStockFisicoPorProdutoAsync(List<int> idsPaióis)
        {
            if (idsPaióis.Count == 0)
                return new Dictionary<int, decimal>();

            var entradas = await _context.EntradasPaiol
                .Where(e => idsPaióis.Contains(e.PaiolId))
                .GroupBy(e => e.ProdutoId)
                .Select(g => new { ProdutoId = g.Key, Total = g.Sum(e => e.Quantidade) })
                .ToListAsync();
            var saidas = await _context.SaidasPaiol
                .Where(s => idsPaióis.Contains(s.PaiolId))
                .GroupBy(s => s.ProdutoId)
                .Select(g => new { ProdutoId = g.Key, Total = g.Sum(s => s.Quantidade) })
                .ToListAsync();

            var resultado = new Dictionary<int, decimal>();
            foreach (var e in entradas)
                resultado[e.ProdutoId] = e.Total;
            foreach (var s in saidas)
                resultado[s.ProdutoId] = resultado.GetValueOrDefault(s.ProdutoId) - s.Total;
            return resultado;
        }

        /// <summary>Catálogo com quantidade em stock (igual ao catálogo mas com coluna Stock nos paióis com acesso).</summary>
        public async Task<IActionResult> Stock(string? pesquisa, string? classificacao, string? grupoCompatibilidade, string? filtroTecnico, string? calibre)
        {
            ViewData["Pesquisa"] = pesquisa;
            ViewData["Classificacao"] = classificacao;
            ViewData["GrupoCompatibilidade"] = grupoCompatibilidade;
            ViewData["FiltroTecnico"] = filtroTecnico;
            ViewData["Calibre"] = calibre;

            var idsAcesso = await ObterPaiolIdsComAcessoAsync();
            var stockPorProduto = await ObterStockFisicoPorProdutoAsync(idsAcesso);

            var query = _context.Produtos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(pesquisa))
                query = query.Where(p => p.Nome.Contains(pesquisa));
            if (!string.IsNullOrEmpty(classificacao))
                query = query.Where(p => p.FamiliaRisco == classificacao);
            if (!string.IsNullOrEmpty(grupoCompatibilidade))
                query = query.Where(p => p.GrupoCompatibilidade == grupoCompatibilidade);
            if (!string.IsNullOrEmpty(filtroTecnico))
                query = query.Where(p => p.FiltroTecnico == filtroTecnico);
            if (!string.IsNullOrEmpty(calibre))
                query = query.Where(p => p.Calibre == calibre);

            var lista = await query.OrderBy(p => p.Nome).ToListAsync();
            ViewData["StockPorProduto"] = stockPorProduto;
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

                var userIdsEntradas = entradas.Where(e => !string.IsNullOrEmpty(e.FuncionarioRegistouUserId)).Select(e => e.FuncionarioRegistouUserId!).Distinct().ToList();
                var nomesEntradas = new Dictionary<string, string>();
                foreach (var uid in userIdsEntradas)
                {
                    var u = await _userManager.FindByIdAsync(uid);
                    nomesEntradas[uid] = u?.UserName ?? uid;
                }
                ViewData["NomesUtilizadoresEntradas"] = nomesEntradas;
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

                var userIds = saidas.Where(s => !string.IsNullOrEmpty(s.FuncionarioRetirouUserId)).Select(s => s.FuncionarioRetirouUserId!).Distinct().ToList();
                var nomesSaidas = new Dictionary<string, string>();
                foreach (var uid in userIds)
                {
                    var u = await _userManager.FindByIdAsync(uid);
                    nomesSaidas[uid] = u?.UserName ?? uid;
                }
                ViewData["NomesUtilizadoresSaidas"] = nomesSaidas;
            }

            return View();
        }

        // GET: Paiol/Gestao — CRUD completo; apenas Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Gestao()
        {
            var lista = await _context.Paiol.OrderBy(p => p.Nome).ToListAsync();
            var ocupacao = await CalcularOcupacaoPorPaiolAsync(lista);
            var viewModel = lista.Select(p => new PaiolComOcupacaoViewModel
            {
                Paiol = p,
                MleAtual = ocupacao.GetValueOrDefault(p.Id).MleAtual,
                PercentagemOcupacao = ocupacao.GetValueOrDefault(p.Id).PercentagemOcupacao
            }).ToList();
            return View(viewModel);
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
                    return p == null ? null : new CargaPaiolItem { ProdutoId = p.Id, ProdutoNome = p.Nome, Quantidade = kv.Value, NEMPorUnidade = p.NEMPorUnidade, Divisao = p.FamiliaRisco ?? "" };
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

            var paiol = await _context.Paiol.Include(p => p.DocumentosExtras).FirstOrDefaultAsync(m => m.Id == id);
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
                    return p == null ? null : new CargaPaiolItem { ProdutoId = p.Id, ProdutoNome = p.Nome, Quantidade = kv.Value, NEMPorUnidade = p.NEMPorUnidade, Divisao = p.FamiliaRisco ?? "" };
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
        public async Task<IActionResult> Create([Bind("Id,Nome,Localizacao,LimiteMLE,PerfilRisco,Estado,NumeroLicenca,DataValidadeLicenca")] Paiol paiol, string[]? CargosAcesso, List<DocumentoExtraInput>? documentosExtras)
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
                var pastaBase = Path.Combine(_env.WebRootPath, PastaDocumentosPaiol, paiol.Id.ToString());
                if (Directory.Exists(pastaBase) == false)
                    Directory.CreateDirectory(pastaBase);
                if (documentosExtras != null)
                {
                    var idx = 0;
                    foreach (var ext in documentosExtras)
                    {
                        if (ext?.Ficheiro != null && FicheiroPermitido(ext.Ficheiro.FileName))
                        {
                            var nome = string.IsNullOrWhiteSpace(ext.Nome) ? "Documento " + (idx + 1) : ext.Nome.Trim();
                            if (nome.Length > 100) nome = nome[..100];
                            var caminho = await GuardarFicheiro(ext.Ficheiro, pastaBase, "doc_" + Guid.NewGuid().ToString("N")[..8]);
                            _context.PaiolDocumentoExtras.Add(new PaiolDocumentoExtra { PaiolId = paiol.Id, Nome = nome, Caminho = caminho });
                            idx++;
                        }
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

            var paiol = await _context.Paiol.Include(p => p.DocumentosExtras).FirstOrDefaultAsync(p => p.Id == id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Localizacao,LimiteMLE,PerfilRisco,Estado,NumeroLicenca,DataValidadeLicenca")] Paiol paiol, string[]? CargosAcesso, List<DocumentoExtraInput>? documentosExtras, List<int>? RemoverDocumentoExtraIds)
        {
            if (id != paiol.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (RemoverDocumentoExtraIds != null && RemoverDocumentoExtraIds.Count > 0)
                    {
                        var aRemover = await _context.PaiolDocumentoExtras
                            .Where(e => e.PaiolId == id && RemoverDocumentoExtraIds.Contains(e.Id))
                            .ToListAsync();
                        foreach (var e in aRemover)
                        {
                            var caminhoFisico = Path.Combine(_env.WebRootPath, e.Caminho);
                            if (System.IO.File.Exists(caminhoFisico))
                            {
                                try { System.IO.File.Delete(caminhoFisico); } catch { }
                            }
                            _context.PaiolDocumentoExtras.Remove(e);
                        }
                        await _context.SaveChangesAsync();
                    }

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

                    var pastaBase = Path.Combine(_env.WebRootPath, PastaDocumentosPaiol, id.ToString());
                    if (Directory.Exists(pastaBase) == false)
                        Directory.CreateDirectory(pastaBase);
                    if (documentosExtras != null)
                    {
                        var idx = 0;
                        foreach (var ext in documentosExtras)
                        {
                            if (ext?.Ficheiro != null && FicheiroPermitido(ext.Ficheiro.FileName))
                            {
                                var nome = string.IsNullOrWhiteSpace(ext.Nome) ? "Documento " + (idx + 1) : ext.Nome.Trim();
                                if (nome.Length > 100) nome = nome[..100];
                                var caminho = await GuardarFicheiro(ext.Ficheiro, pastaBase, "doc_" + Guid.NewGuid().ToString("N")[..8]);
                                _context.PaiolDocumentoExtras.Add(new PaiolDocumentoExtra { PaiolId = id, Nome = nome, Caminho = caminho });
                                idx++;
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
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
                var pastaBase = Path.Combine(_env.WebRootPath, PastaDocumentosPaiol, id.ToString());
                if (Directory.Exists(pastaBase))
                {
                    try { Directory.Delete(pastaBase, recursive: true); } catch { }
                }
                _context.Paiol.Remove(paiol);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Gestao));
        }

        public IActionResult Download(int id, int extraId)
        {
            var extra = _context.PaiolDocumentoExtras.AsNoTracking().FirstOrDefault(e => e.Id == extraId && e.PaiolId == id);
            if (extra == null)
                return NotFound();
            return ServirFicheiro(extra.Caminho);
        }

        private IActionResult ServirFicheiro(string caminhoRelativo)
        {
            var caminhoFisico = Path.Combine(_env.WebRootPath, caminhoRelativo);
            if (!System.IO.File.Exists(caminhoFisico))
                return NotFound();
            var ext = Path.GetExtension(caminhoRelativo).ToLowerInvariant();
            var contentType = ext switch { ".pdf" => "application/pdf", ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", _ => "application/octet-stream" };
            var nomeFicheiro = Path.GetFileName(caminhoRelativo);
            Response.Headers["Content-Disposition"] = "inline; filename=\"" + nomeFicheiro.Replace("\"", "\\\"") + "\"";
            return PhysicalFile(caminhoFisico, contentType);
        }

        private static bool FicheiroPermitido(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            return !string.IsNullOrEmpty(ext) && ExtensoesPermitidas.Contains(ext.ToLowerInvariant());
        }

        private async Task<string> GuardarFicheiro(IFormFile ficheiro, string pastaBase, string prefixo)
        {
            var ext = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            var nomeUnico = $"{prefixo}_{Guid.NewGuid():N}{ext}";
            var caminhoFisico = Path.Combine(pastaBase, nomeUnico);
            await using var stream = new FileStream(caminhoFisico, FileMode.Create);
            await ficheiro.CopyToAsync(stream);
            var idPasta = Path.GetFileName(pastaBase.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return Path.Combine(PastaDocumentosPaiol, idPasta, nomeUnico).Replace('\\', '/');
        }

        private async Task<bool> PaiolExistsAsync(int id)
        {
            return await _context.Paiol.AnyAsync(e => e.Id == id);
        }
    }
}
