using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Tutorial Class 3: CRUD de Produto (NEM por unidade, família de risco). Class 8: [Authorize].
    /// </summary>
    [Authorize]
    public class ProdutosController : Controller
    {
        private readonly FinalprojContext _context;

        public ProdutosController(FinalprojContext context)
        {
            _context = context;
        }

        /// <summary> Catálogo com barra de pesquisa e filtros (classificação, filtro técnico, calibre). Lista todos os produtos. </summary>
        public async Task<IActionResult> Index(string? pesquisa, string? classificacao, string? filtroTecnico, string? calibre)
        {
            ViewData["Pesquisa"] = pesquisa;
            ViewData["Classificacao"] = classificacao;
            ViewData["FiltroTecnico"] = filtroTecnico;
            ViewData["Calibre"] = calibre;

            var query = _context.Produtos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(pesquisa))
                query = query.Where(p => p.Nome.Contains(pesquisa));
            if (!string.IsNullOrEmpty(classificacao))
                query = query.Where(p => p.FamiliaRisco == classificacao);
            if (!string.IsNullOrEmpty(filtroTecnico))
                query = query.Where(p => p.FiltroTecnico == filtroTecnico);
            if (!string.IsNullOrEmpty(calibre))
                query = query.Where(p => p.Calibre == calibre);

            var lista = await query.OrderBy(p => p.Nome).ToListAsync();
            return View(lista);
        }

        /// <summary> Gerir produtos com o mesmo sistema de subdivisão do catálogo (Class 3 – listagem filtrada). </summary>
        public async Task<IActionResult> Gerir(string? pesquisa, string? classificacao, string? filtroTecnico, string? calibre)
        {
            ViewData["Pesquisa"] = pesquisa ?? "";
            ViewData["Classificacao"] = classificacao ?? "";
            ViewData["FiltroTecnico"] = filtroTecnico ?? "";
            ViewData["Calibre"] = calibre ?? "";

            var query = _context.Produtos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(pesquisa))
                query = query.Where(p => p.Nome.Contains(pesquisa));
            if (!string.IsNullOrEmpty(classificacao))
                query = query.Where(p => p.FamiliaRisco == classificacao);
            if (!string.IsNullOrEmpty(filtroTecnico))
                query = query.Where(p => p.FiltroTecnico == filtroTecnico);
            if (!string.IsNullOrEmpty(calibre))
                query = query.Where(p => p.Calibre == calibre);

            var lista = await query.OrderBy(p => p.Nome).ToListAsync();
            return View(lista);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();
            return View(produto);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["FamiliaRisco"] = new SelectList(ConstantesPaiol.FamiliasParaDropdown(), "Value", "Text");
            ViewData["FiltroTecnico"] = new SelectList(ConstantesCatalogo.FiltrosTecnicosParaDropdown(), "Value", "Text");
            ViewData["Calibre"] = new SelectList(ConstantesCatalogo.CalibresParaDropdown(), "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Nome,NEMPorUnidade,FamiliaRisco,FiltroTecnico,Calibre")] Produto produto)
        {
            if (ModelState.IsValid)
            {
                _context.Add(produto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FamiliaRisco"] = new SelectList(ConstantesPaiol.FamiliasParaDropdown(), "Value", "Text", produto.FamiliaRisco);
            ViewData["FiltroTecnico"] = new SelectList(ConstantesCatalogo.FiltrosTecnicosParaDropdown(), "Value", "Text", produto.FiltroTecnico);
            ViewData["Calibre"] = new SelectList(ConstantesCatalogo.CalibresParaDropdown(), "Value", "Text", produto.Calibre);
            return View(produto);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null) return NotFound();
            ViewData["FamiliaRisco"] = new SelectList(ConstantesPaiol.FamiliasParaDropdown(), "Value", "Text", produto.FamiliaRisco);
            ViewData["FiltroTecnico"] = new SelectList(ConstantesCatalogo.FiltrosTecnicosParaDropdown(), "Value", "Text", produto.FiltroTecnico);
            ViewData["Calibre"] = new SelectList(ConstantesCatalogo.CalibresParaDropdown(), "Value", "Text", produto.Calibre);
            return View(produto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,NEMPorUnidade,FamiliaRisco,Unidade,FiltroTecnico,Calibre")] Produto produto)
        {
            if (id != produto.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Produtos.AnyAsync(e => e.Id == produto.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Gerir));
            }
            ViewData["FamiliaRisco"] = new SelectList(ConstantesPaiol.FamiliasParaDropdown(), "Value", "Text", produto.FamiliaRisco);
            ViewData["FiltroTecnico"] = new SelectList(ConstantesCatalogo.FiltrosTecnicosParaDropdown(), "Value", "Text", produto.FiltroTecnico);
            ViewData["Calibre"] = new SelectList(ConstantesCatalogo.CalibresParaDropdown(), "Value", "Text", produto.Calibre);
            return View(produto);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var produto = await _context.Produtos.FirstOrDefaultAsync(m => m.Id == id);
            if (produto == null) return NotFound();
            return View(produto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto != null)
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Gerir));
        }
    }
}
