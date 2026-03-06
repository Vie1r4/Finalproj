using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly FinalprojContext _context;
        private readonly IWebHostEnvironment _env;
        private const string PastaDocumentosClientes = "Documentos/Clientes";
        private static readonly string[] ExtensoesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png" };

        public ClientesController(FinalprojContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(string? pesquisa, string? ordenar, CancellationToken cancellationToken = default)
        {
            var query = _context.Clientes.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                var termo = pesquisa.Trim();
                query = query.Where(c =>
                    c.Nome.Contains(termo) ||
                    (c.Email != null && c.Email.Contains(termo)) ||
                    (c.Telefone != null && c.Telefone.Contains(termo)) ||
                    (c.NIF != null && c.NIF.Contains(termo)));
            }

            query = (ordenar ?? "nome") switch
            {
                "email" => query.OrderBy(c => c.Email ?? ""),
                "recentes" => query.OrderByDescending(c => c.DataRegisto ?? DateTime.MinValue),
                _ => query.OrderBy(c => c.Nome)
            };

            var lista = await query.ToListAsync(cancellationToken);

            ViewData["Pesquisa"] = pesquisa ?? string.Empty;
            ViewData["Ordenar"] = ordenar ?? "nome";
            return View(lista);
        }

        private const int HistoricoEncomendasPageSize = 15;

        public async Task<IActionResult> Details(int? id, int historicoPagina = 1, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var cliente = await _context.Clientes.AsNoTracking().Include(c => c.DocumentosExtras).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (cliente == null)
                return NotFound();

            var encomendasAtivas = await _context.Encomendas
                .AsNoTracking()
                .Include(e => e.Itens)
                .ThenInclude(i => i.Produto)
                .Where(e => e.ClienteId == id && ConstantesEncomenda.EstadosComReserva.Contains(e.Estado))
                .OrderByDescending(e => e.DataCriacao)
                .ToListAsync(cancellationToken);

            var queryHistorico = _context.Encomendas
                .AsNoTracking()
                .Where(e => e.ClienteId == id && (e.Estado == ConstantesEncomenda.CONCLUIDA || e.Estado == ConstantesEncomenda.REJEITADA))
                .OrderByDescending(e => e.DataConclusao ?? e.DataCriacao);

            var totalHistorico = await queryHistorico.CountAsync(cancellationToken);
            var totalPaginasHistorico = totalHistorico == 0 ? 1 : (int)Math.Ceiling(totalHistorico / (double)HistoricoEncomendasPageSize);
            historicoPagina = Math.Clamp(historicoPagina, 1, totalPaginasHistorico);

            var encomendasHistorico = await queryHistorico
                .Skip((historicoPagina - 1) * HistoricoEncomendasPageSize)
                .Take(HistoricoEncomendasPageSize)
                .ToListAsync(cancellationToken);

            ViewData["EncomendasAtivas"] = encomendasAtivas;
            ViewData["EncomendasHistorico"] = encomendasHistorico;
            ViewData["HistoricoPagina"] = historicoPagina;
            ViewData["TotalPaginasHistorico"] = totalPaginasHistorico;
            ViewData["TotalHistorico"] = totalHistorico;
            return View(cliente);
        }

        public IActionResult Create()
        {
            ViewData["TiposCliente"] = ConstantesFuncionariosClientes.TiposClienteParaDropdown();
            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nome,TipoCliente,NIF,Email,Telefone,Morada,Notas")] Cliente cliente,
            List<DocumentoExtraInput>? documentosExtras,
            CancellationToken cancellationToken = default)
        {
            if (ModelState.IsValid)
            {
                cliente.DataRegisto = DateTime.UtcNow;
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync(cancellationToken);

                var pastaBase = Path.Combine(_env.WebRootPath, PastaDocumentosClientes, cliente.Id.ToString());
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
                            var caminho = await GuardarFicheiro(ext.Ficheiro, pastaBase, "doc_" + idx);
                            _context.ClienteDocumentoExtras.Add(new ClienteDocumentoExtra { ClienteId = cliente.Id, Nome = nome, Caminho = caminho });
                            idx++;
                        }
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                }
                TempData["ClienteCriado"] = true;
                return RedirectToAction(nameof(Index));
            }
            ViewData["TiposCliente"] = ConstantesFuncionariosClientes.TiposClienteParaDropdown();
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var item = await _context.Clientes.Include(c => c.DocumentosExtras).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (item == null)
                return NotFound();
            ViewData["TiposCliente"] = ConstantesFuncionariosClientes.TiposClienteParaDropdown();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nome,TipoCliente,NIF,Email,Telefone,Morada,Notas,DataRegisto")] Cliente cliente,
            List<DocumentoExtraInput>? documentosExtras,
            List<int>? removerDocumentoExtraIds,
            CancellationToken cancellationToken = default)
        {
            if (id != cliente.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    if (removerDocumentoExtraIds != null && removerDocumentoExtraIds.Count > 0)
                    {
                        var aRemover = await _context.ClienteDocumentoExtras
                            .Where(e => e.ClienteId == id && removerDocumentoExtraIds.Contains(e.Id))
                            .ToListAsync(cancellationToken);
                        foreach (var e in aRemover)
                        {
                            var caminhoFisico = Path.Combine(_env.WebRootPath, e.Caminho);
                            if (System.IO.File.Exists(caminhoFisico))
                            {
                                try { System.IO.File.Delete(caminhoFisico); } catch { }
                            }
                            _context.ClienteDocumentoExtras.Remove(e);
                        }
                    }

                    var existente = await _context.Clientes.FindAsync(id);
                    if (existente == null)
                        return NotFound();
                    existente.Nome = cliente.Nome;
                    existente.TipoCliente = cliente.TipoCliente;
                    existente.NIF = cliente.NIF;
                    existente.Email = cliente.Email;
                    existente.Telefone = cliente.Telefone;
                    existente.Morada = cliente.Morada;
                    existente.Notas = cliente.Notas;

                    var pastaBase = Path.Combine(_env.WebRootPath, PastaDocumentosClientes, id.ToString());
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
                                _context.ClienteDocumentoExtras.Add(new ClienteDocumentoExtra { ClienteId = id, Nome = nome, Caminho = caminho });
                                idx++;
                            }
                        }
                    }
                    await _context.SaveChangesAsync(cancellationToken);
                    TempData["ClienteEditado"] = true;
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Clientes.AnyAsync(e => e.Id == cliente.Id, cancellationToken))
                        return NotFound();
                    throw;
                }
            }
            ViewData["TiposCliente"] = ConstantesFuncionariosClientes.TiposClienteParaDropdown();
            return View(cliente);
        }

        public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var item = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
        {
            var item = await _context.Clientes.FindAsync(id);
            if (item != null)
            {
                var pastaBase = Path.Combine(_env.WebRootPath, PastaDocumentosClientes, id.ToString());
                if (Directory.Exists(pastaBase))
                {
                    try { Directory.Delete(pastaBase, recursive: true); } catch { }
                }
                _context.Clientes.Remove(item);
                await _context.SaveChangesAsync(cancellationToken);
                TempData["ClienteEliminado"] = true;
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Download(int id, int extraId)
        {
            var extra = _context.ClienteDocumentoExtras.AsNoTracking().FirstOrDefault(e => e.Id == extraId && e.ClienteId == id);
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

        private static async Task<string> GuardarFicheiro(IFormFile ficheiro, string pastaBase, string prefixo)
        {
            var ext = Path.GetExtension(ficheiro.FileName).ToLowerInvariant();
            var nomeUnico = $"{prefixo}_{Guid.NewGuid():N}{ext}";
            var caminhoFisico = Path.Combine(pastaBase, nomeUnico);
            await using var stream = new FileStream(caminhoFisico, FileMode.Create);
            await ficheiro.CopyToAsync(stream);
            var idPasta = Path.GetFileName(pastaBase.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            return Path.Combine(PastaDocumentosClientes, idPasta, nomeUnico).Replace('\\', '/');
        }
    }
}
