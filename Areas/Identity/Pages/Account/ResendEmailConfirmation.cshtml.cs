using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Finalproj.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResendEmailConfirmationModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public ResendEmailConfirmationModel(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = null!;

        public class InputModel
        {
            [Required(ErrorMessage = "O email é obrigatório.")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet(string? email = null)
        {
            Input = new InputModel { Email = email ?? "" };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Não existe nenhuma conta com este email.");
                return Page();
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Este email já está confirmado. Pode iniciar sessão.");
                return Page();
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = Url.Page("/Account/ConfirmEmail", pageHandler: null, values: new { area = "Identity", userId = user.Id, code }, protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(Input.Email, "Confirmar o seu email – PIROFAFE",
                $"Por favor confirme a sua conta <a href='{callbackUrl}'>clicando aqui</a>. Se não pediu este email, ignore-o.");

            TempData["EmailReenviado"] = true;
            return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
        }
    }
}
