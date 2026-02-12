using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Tutorial Class 8: "The authorization can be done using Roles" e "Adapt the Menu to show only the options allowed to the role of the user authenticated."
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private static readonly string[] RolesDisponiveis = { "Admin", "Armazém", "Técnico", "Comercial" };

        public AdminController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Utilizadores()
        {
            var utilizadores = new List<UtilizadorComRolesViewModel>();
            foreach (var user in _userManager.Users.OrderBy(u => u.UserName))
            {
                var roles = await _userManager.GetRolesAsync(user);
                utilizadores.Add(new UtilizadorComRolesViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Roles = roles
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
            var model = new EditarUtilizadorRolesViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
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
            var rolesAtuais = await _userManager.GetRolesAsync(user);
            foreach (var role in RolesDisponiveis)
            {
                var deveTer = model.Roles?.Any(r => r.Nome == role && r.Atribuido) ?? false;
                if (deveTer && !rolesAtuais.Contains(role))
                    await _userManager.AddToRoleAsync(user, role);
                else if (!deveTer && rolesAtuais.Contains(role))
                    await _userManager.RemoveFromRoleAsync(user, role);
            }
            return RedirectToAction(nameof(Utilizadores));
        }
    }
}
