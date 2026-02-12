using System.Diagnostics;
using Finalproj.Data;
using Finalproj.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Controllers
{
    /// <summary>
    /// Tutorial Class 8: "The [Authorize] filter, says that to access this controller the user needs to be authenticated.
    /// If he is not authenticated then is redirect to the login page." e "We can authorize some actions to be access to anonymous users."
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly FinalprojContext _context;
        private static readonly string[] RolesDisponiveis = { "Admin", "Armazém", "Técnico", "Comercial" };

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            FinalprojContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _context = context;
        }

        /// <summary>
        /// Página inicial – requer autenticação (Tutorial Class 8: sem sessão não navega).
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Página de confirmação para apagar todos os dados do site (contas e dados adjacentes). Acessível a qualquer utilizador autenticado.
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult LimparDados()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("LimparDados")]
        public async Task<IActionResult> LimparDadosConfirmar()
        {
            foreach (var user in _userManager.Users.ToList())
                await _userManager.DeleteAsync(user);

            foreach (var role in _roleManager.Roles.ToList())
                await _roleManager.DeleteAsync(role);

            await _context.Paiol.ExecuteDeleteAsync();
            await _context.Perfis.ExecuteDeleteAsync();
            await _context.SaveChangesAsync();

            foreach (var roleName in RolesDisponiveis)
                await _roleManager.CreateAsync(new IdentityRole(roleName));

            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Tutorial Class 7: "Add the Preferences action" – "use cookies to store user preferences locally in the browser",
        /// "change the application presentation between light mode and dark mode". Opção só para utilizadores autenticados.
        /// </summary>
        [HttpGet]
        public IActionResult Preferencias()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Preferencias(string tema, string? returnAction = null, string? returnController = null)
        {
            var opcoes = new CookieOptions { IsEssential = true, Expires = DateTimeOffset.UtcNow.AddYears(1) };
            Response.Cookies.Append("Theme", tema ?? "Light", opcoes);
            if (!string.IsNullOrEmpty(returnAction) && returnAction == nameof(Perfil))
                return RedirectToAction(nameof(Perfil));
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Página de perfil do utilizador autenticado (Tutorial Class 8: dados adicionais em Perfil, Roles).
        /// Email e nome de utilizador são só leitura; Nome e Telefone editáveis.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));

            var perfil = await _context.Perfis.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            var model = new PerfilEditViewModel
            {
                UserName = user.UserName ?? user.Email ?? "",
                Email = user.Email ?? "",
                Nome = perfil?.Nome,
                Telefone = perfil?.Telefone,
                Roles = roles.ToList(),
                DataRegisto = perfil?.DataRegisto
            };
            ViewData["AlterarPasswordViewModel"] = new AlterarPasswordViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(PerfilEditViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                var perfil = await _context.Perfis.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (perfil == null)
                {
                    perfil = new Perfil { UserId = user.Id, DataRegisto = DateTime.UtcNow };
                    _context.Perfis.Add(perfil);
                }
                perfil.Nome = model.Nome;
                perfil.Telefone = model.Telefone;
                await _context.SaveChangesAsync();
                TempData["PerfilGuardado"] = true;
                return RedirectToAction(nameof(Perfil));
            }

            model.UserName = user.UserName ?? user.Email ?? "";
            model.Email = user.Email ?? "";
            var roles = await _userManager.GetRolesAsync(user);
            model.Roles = roles.ToList();
            var perfilReload = await _context.Perfis.FirstOrDefaultAsync(p => p.UserId == user.Id);
            model.DataRegisto = perfilReload?.DataRegisto;
            ViewData["AlterarPasswordViewModel"] = new AlterarPasswordViewModel();
            return View(model);
        }

        /// <summary>
        /// Alterar palavra-passe a partir da página de perfil (Identity).
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarPassword(AlterarPasswordViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Perfil));

            if (ModelState.IsValid)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.PasswordAtual, model.NovaPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    TempData["PasswordAlterada"] = true;
                    return RedirectToAction(nameof(Perfil));
                }
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            var perfilModel = await ObterPerfilEditViewModelAsync(user);
            ViewData["AlterarPasswordViewModel"] = model;
            return View("Perfil", perfilModel);
        }

        private async Task<PerfilEditViewModel> ObterPerfilEditViewModelAsync(IdentityUser user)
        {
            var perfil = await _context.Perfis.FirstOrDefaultAsync(p => p.UserId == user.Id);
            var roles = await _userManager.GetRolesAsync(user);
            return new PerfilEditViewModel
            {
                UserName = user.UserName ?? user.Email ?? "",
                Email = user.Email ?? "",
                Nome = perfil?.Nome,
                Telefone = perfil?.Telefone,
                Roles = roles.ToList(),
                DataRegisto = perfil?.DataRegisto
            };
        }
    }
}
