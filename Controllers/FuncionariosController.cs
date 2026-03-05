using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    [Authorize]
    public class FuncionariosController : Controller
    {
        private readonly FinalprojContext _context;

        public FuncionariosController(FinalprojContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? pesquisa, string? ordenar, string? cargo, CancellationToken cancellationToken = default)
        {
            var query = _context.Funcionarios.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(pesquisa))
            {
                var termo = pesquisa.Trim();
                query = query.Where(f =>
                    f.NomeCompleto.Contains(termo) ||
                    (f.Email != null && f.Email.Contains(termo)) ||
                    (f.Telefone != null && f.Telefone.Contains(termo)) ||
                    (f.NIF != null && f.NIF.Contains(termo)));
            }

            if (!string.IsNullOrEmpty(cargo))
                query = query.Where(f => f.Cargo == cargo);

            query = (ordenar ?? "nome") switch
            {
                "email" => query.OrderBy(f => f.Email ?? ""),
                "recentes" => query.OrderByDescending(f => f.DataRegisto ?? DateTime.MinValue),
                _ => query.OrderBy(f => f.NomeCompleto)
            };

            var lista = await query.ToListAsync(cancellationToken);

            ViewData["Pesquisa"] = pesquisa ?? string.Empty;
            ViewData["Ordenar"] = ordenar ?? "nome";
            ViewData["Cargo"] = cargo ?? string.Empty;
            ViewData["Cargos"] = ConstantesFuncionariosClientes.CargosParaDropdown();
            return View(lista);
        }

        public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var item = await _context.Funcionarios.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PreencherDropdownCargo(null);
            return View(new Funcionario());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("NomeCompleto,NIF,DataNascimento,Email,Telefone,Morada,CodigoPostal,Localidade,Cargo,DataAdmissao,DataSaida,NumeroSegurancaSocial,IBAN,Notas")] Funcionario funcionario,
            CancellationToken cancellationToken = default)
        {
            if (ModelState.IsValid)
            {
                funcionario.DataRegisto = DateTime.UtcNow;
                _context.Funcionarios.Add(funcionario);
                await _context.SaveChangesAsync(cancellationToken);
                TempData["FuncionarioCriado"] = true;
                return RedirectToAction(nameof(Details), new { id = funcionario.Id });
            }
            PreencherDropdownCargo(funcionario.Cargo);
            return View(funcionario);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var item = await _context.Funcionarios.FindAsync(id);
            if (item == null)
                return NotFound();
            PreencherDropdownCargo(item.Cargo);
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,NomeCompleto,NIF,DataNascimento,Email,Telefone,Morada,CodigoPostal,Localidade,Cargo,DataAdmissao,DataSaida,NumeroSegurancaSocial,IBAN,Notas,UserId,DataRegisto")] Funcionario funcionario,
            CancellationToken cancellationToken = default)
        {
            if (id != funcionario.Id)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(funcionario);
                    await _context.SaveChangesAsync(cancellationToken);
                    TempData["FuncionarioEditado"] = true;
                    return RedirectToAction(nameof(Details), new { id = funcionario.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Funcionarios.AnyAsync(e => e.Id == funcionario.Id, cancellationToken))
                        return NotFound();
                    throw;
                }
            }
            PreencherDropdownCargo(funcionario.Cargo);
            return View(funcionario);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id, CancellationToken cancellationToken = default)
        {
            if (id == null)
                return NotFound();
            var item = await _context.Funcionarios.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken = default)
        {
            var item = await _context.Funcionarios.FindAsync(id);
            if (item != null)
            {
                _context.Funcionarios.Remove(item);
                await _context.SaveChangesAsync(cancellationToken);
                TempData["FuncionarioEliminado"] = true;
            }
            return RedirectToAction(nameof(Index));
        }

        private void PreencherDropdownCargo(string? valorSelecionado)
        {
            ViewData["Cargo"] = new SelectList(
                ConstantesFuncionariosClientes.CargosParaDropdown(),
                "Value", "Text",
                valorSelecionado);
        }
    }
}
