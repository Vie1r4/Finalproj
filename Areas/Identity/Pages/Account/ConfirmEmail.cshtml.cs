using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Finalproj.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmEmailModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public bool Succeeded { get; set; }
        public string? StatusMessage { get; set; }
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string? userId, string? code, string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                StatusMessage = "Link de confirmação inválido.";
                Succeeded = false;
                return Page();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                StatusMessage = "Utilizador não encontrado.";
                Succeeded = false;
                return Page();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);
            Succeeded = result.Succeeded;
            StatusMessage = result.Succeeded ? "Email confirmado." : "Erro ao confirmar o email.";
            return Page();
        }
    }
}
