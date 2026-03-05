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

        public ClientesController(FinalprojContext context)
        {
            _context = context;
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
            var cliente = await _context.Clientes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (cliente == null)
                return NotFound();

            var encomendasAtivas = await _context.Encomendas
                .AsNoTracking()
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
            return View(new Cliente());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nome,Email,Telefone,Notas")] Cliente cliente,
            CancellationToken cancellationToken = default)
        {
            if (ModelState.IsValid)
            {
                cliente.TipoCliente = "Particular";
                cliente.DataRegisto = DateTime.UtcNow;
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync(cancellationToken);
                TempData["ClienteCriado"] = true;
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var item = await _context.Clientes.FindAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nome,Email,Telefone,Notas,DataRegisto")] Cliente cliente,
            CancellationToken cancellationToken = default)
        {
            if (id != cliente.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    var existente = await _context.Clientes.FindAsync(id);
                    if (existente == null)
                        return NotFound();
                    existente.Nome = cliente.Nome;
                    existente.Email = cliente.Email;
                    existente.Telefone = cliente.Telefone;
                    existente.Notas = cliente.Notas;
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
                _context.Clientes.Remove(item);
                await _context.SaveChangesAsync(cancellationToken);
                TempData["ClienteEliminado"] = true;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
