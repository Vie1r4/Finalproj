using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Registo de saída de material do paiol. Só aparecem paióis a que o utilizador tem acesso (por cargo).
    /// </summary>
    [Authorize]
    public class SaidaPaiolController : Controller
    {
        private readonly FinalprojContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SaidaPaiolController(FinalprojContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary> GET: a única forma de retirar material é abrir o paiol, escolher o produto e depois indicar a quantidade. Sem paiolId e produtoId redireciona para o Armazém. </summary>
        public async Task<IActionResult> Registar(int? paiolId, int? produtoId)
        {
            if (!paiolId.HasValue || !produtoId.HasValue)
                return RedirectToAction("Index", "Paiol");

            var user = await _userManager.GetUserAsync(User);
            var roles = user == null ? Array.Empty<string>() : (await _userManager.GetRolesAsync(user)).ToArray();
            var idsAcesso = await _context.PaiolAcessos
                .Where(a => roles.Contains(a.RoleName))
                .Select(a => a.PaiolId)
                .Distinct()
                .ToListAsync();
            if (!idsAcesso.Contains(paiolId.Value))
                return Forbid();

            var paiol = await _context.Paiol.FindAsync(paiolId);
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (paiol == null || produto == null)
                return NotFound();

            var entradas = await _context.EntradasPaiol
                .Where(e => e.PaiolId == paiolId && e.ProdutoId == produtoId)
                .SumAsync(e => e.Quantidade);
            var saidas = await _context.SaidasPaiol
                .Where(s => s.PaiolId == paiolId && s.ProdutoId == produtoId)
                .SumAsync(s => s.Quantidade);
            var stockDisponivel = entradas - saidas;

            ViewData["PaiolNome"] = paiol.Nome;
            ViewData["ProdutoNome"] = produto.Nome;
            ViewData["StockDisponivel"] = stockDisponivel;

            var model = new SaidaPaiolViewModel { PaiolId = paiolId.Value, ProdutoId = produtoId.Value };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar(SaidaPaiolViewModel model)
        {
            var paiol = await _context.Paiol.FindAsync(model.PaiolId);
            var produto = await _context.Produtos.FindAsync(model.ProdutoId);

            if (paiol == null || produto == null)
            {
                ModelState.AddModelError(string.Empty, "Paiol ou produto inválido.");
                return RedirectToAction("Index", "Paiol");
            }

            var entradas = await _context.EntradasPaiol
                .Where(e => e.PaiolId == model.PaiolId && e.ProdutoId == model.ProdutoId)
                .SumAsync(e => e.Quantidade);
            var saidas = await _context.SaidasPaiol
                .Where(s => s.PaiolId == model.PaiolId && s.ProdutoId == model.ProdutoId)
                .SumAsync(s => s.Quantidade);
            var stockDisponivel = entradas - saidas;

            if (model.Quantidade > stockDisponivel)
            {
                ModelState.AddModelError(string.Empty,
                    $"Quantidade indisponível. Stock atual neste paiol: {stockDisponivel:N2}. Não pode retirar {model.Quantidade:N2}.");
                ViewData["PaiolNome"] = paiol.Nome;
                ViewData["ProdutoNome"] = produto.Nome;
                ViewData["StockDisponivel"] = stockDisponivel;
                return View(model);
            }

            _context.SaidasPaiol.Add(new SaidaPaiol
            {
                PaiolId = model.PaiolId,
                ProdutoId = model.ProdutoId,
                Quantidade = model.Quantidade,
                DataSaida = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            TempData["SaidaSucesso"] = $"Saída registada: {model.Quantidade} × {produto.Nome} do paiol {paiol.Nome}.";
            return RedirectToAction("Conteudo", "Paiol", new { id = model.PaiolId });
        }

        /// <summary> O histórico de saídas só é acessível através de Paiol/Movimentos. </summary>
        public IActionResult Index()
        {
            return RedirectToAction("Movimentos", "Paiol", new { tipo = "Saidas" });
        }

            }
}
