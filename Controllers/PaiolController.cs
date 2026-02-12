using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public PaiolController(FinalprojContext context)
        {
            _context = context;
        }

        // GET: Paiol
        public async Task<IActionResult> Index()
        {
            return View(await _context.Paiol.ToListAsync());
        }

        // GET: Paiol/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var paiol = await _context.Paiol.FirstOrDefaultAsync(m => m.Id == id);
            if (paiol == null)
                return NotFound();

            return View(paiol);
        }

        // GET: Paiol/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Paiol/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Localizacao,LimiteMLE")] Paiol paiol)
        {
            if (ModelState.IsValid)
            {
                _context.Add(paiol);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(paiol);
        }

        // GET: Paiol/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var paiol = await _context.Paiol.FindAsync(id);
            if (paiol == null)
                return NotFound();

            return View(paiol);
        }

        // POST: Paiol/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Localizacao,LimiteMLE")] Paiol paiol)
        {
            if (id != paiol.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(paiol);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await PaiolExistsAsync(paiol.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(paiol);
        }

        // GET: Paiol/Delete/5
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var paiol = await _context.Paiol.FindAsync(id);
            if (paiol != null)
            {
                _context.Paiol.Remove(paiol);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> PaiolExistsAsync(int id)
        {
            return await _context.Paiol.AnyAsync(e => e.Id == id);
        }
    }
}
