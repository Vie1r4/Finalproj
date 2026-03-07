using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Tutorial Class 8: "The authorization can be done using Roles" e "Adapt the Menu to show only the options allowed to the role of the user authenticated."
    /// Inclui associação utilizador ↔ funcionário (UserId na ficha do funcionário).
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly FinalprojContext _context;
        private static readonly string[] RolesDisponiveis = { "Admin", "Armazém", "Técnico", "Comercial" };

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, FinalprojContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Utilizadores()
        {
            var utilizadores = new List<UtilizadorComRolesViewModel>();
            var funcionariosPorUserId = await _context.Funcionarios
                .AsNoTracking()
                .Where(f => f.UserId != null)
                .ToDictionaryAsync(f => f.UserId!, f => f.NomeCompleto);
            foreach (var user in _userManager.Users.OrderBy(u => u.UserName))
            {
                var roles = await _userManager.GetRolesAsync(user);
                utilizadores.Add(new UtilizadorComRolesViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Roles = roles,
                    FuncionarioAssociadoNome = user.Id != null && funcionariosPorUserId.TryGetValue(user.Id, out var nome) ? nome : null
                });
            }
            return View(utilizadores);
        }

        public async Task<IActionResult> EditarUtilizador(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);

            // Funcionários que podem ser associados: sem conta ou já com esta conta
            var funcionariosDisponiveis = await _context.Funcionarios
                .AsNoTracking()
                .Where(f => f.UserId == null || f.UserId == user.Id)
                .OrderBy(f => f.NomeCompleto)
                .Select(f => new { f.Id, f.NomeCompleto })
                .ToListAsync();
            var funcionarioAtual = await _context.Funcionarios
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == user.Id);
            var selectList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                funcionariosDisponiveis,
                "Id",
                "NomeCompleto",
                funcionarioAtual?.Id);

            ViewBag.Funcionarios = selectList;

            var model = new EditarUtilizadorRolesViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FuncionarioId = funcionarioAtual?.Id,
                Roles = RolesDisponiveis.Select(r => new RoleItemViewModel { Nome = r, Atribuido = userRoles.Contains(r) }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarUtilizador(string id, EditarUtilizadorRolesViewModel model)
        {
            if (id != model.Id) return NotFound();
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // Roles
            var rolesAtuais = await _userManager.GetRolesAsync(user);
            foreach (var role in RolesDisponiveis)
            {
                var deveTer = model.Roles?.Any(r => r.Nome == role && r.Atribuido) ?? false;
                if (deveTer && !rolesAtuais.Contains(role))
                    await _userManager.AddToRoleAsync(user, role);
                else if (!deveTer && rolesAtuais.Contains(role))
                    await _userManager.RemoveFromRoleAsync(user, role);
            }

            // Associação a funcionário: no máximo um funcionário por utilizador
            var funcionariosComEsteUser = await _context.Funcionarios.Where(f => f.UserId == user.Id).ToListAsync();
            foreach (var f in funcionariosComEsteUser)
                f.UserId = null;
            if (model.FuncionarioId.HasValue)
            {
                var funcionario = await _context.Funcionarios.FindAsync(model.FuncionarioId.Value);
                if (funcionario != null)
                {
                    // Retirar o user de qualquer outro funcionário que o tivesse
                    var outro = await _context.Funcionarios.FirstOrDefaultAsync(f => f.UserId == user.Id && f.Id != funcionario.Id);
                    if (outro != null) outro.UserId = null;
                    funcionario.UserId = user.Id;
                }
            }
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Utilizadores));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUtilizador(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == id)
            {
                TempData["Erro"] = "Não pode eliminar a sua própria conta de administrador.";
                return RedirectToAction(nameof(Utilizadores));
            }

            var funcionarios = await _context.Funcionarios.Where(f => f.UserId == id).ToListAsync();
            foreach (var f in funcionarios)
            {
                f.UserId = null;
            }

            var clientes = await _context.Clientes.Where(c => c.UserId == id).ToListAsync();
            foreach (var c in clientes)
            {
                c.UserId = null;
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["Erro"] = "Não foi possível eliminar o utilizador.";
            }
            else
            {
                TempData["Sucesso"] = "Utilizador eliminado com sucesso.";
            }

            return RedirectToAction(nameof(Utilizadores));
        }
    }
}
