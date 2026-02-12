using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Finalproj.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Tutorial Class 8: Login com UserName; verificação de email; [AllowAnonymous].
    /// </summary>
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "O nome de utilizador é obrigatório.")]
            [Display(Name = "Nome de utilizador")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-passe")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Manter sessão iniciada")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                    return LocalRedirect(returnUrl);
                var user = await _userManager.FindByNameAsync(Input.UserName);
                if (user != null && !await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError(string.Empty, "Deve confirmar o seu email antes de iniciar sessão.");
                    ViewData["EmailNaoConfirmado"] = user.Email;
                    return Page();
                }
                ModelState.AddModelError(string.Empty, "Tentativa de login inválida.");
            }
            return Page();
        }
    }
}
