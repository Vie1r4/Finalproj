using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Finalproj.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        public string Email { get; set; } = string.Empty;
        public string? ReturnUrl { get; set; }

        public IActionResult OnGet(string? email, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToPage("/Index");
            Email = email;
            ReturnUrl = returnUrl;
            return Page();
        }
    }
}
