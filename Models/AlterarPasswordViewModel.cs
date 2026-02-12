using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models
{
    /// <summary>
    /// Formulário para alterar a palavra-passe na página de perfil (Identity).
    /// </summary>
    public class AlterarPasswordViewModel
    {
        [Required(ErrorMessage = "A palavra-passe atual é obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe atual")]
        public string PasswordAtual { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova palavra-passe é obrigatória.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A palavra-passe deve ter entre 6 e 100 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova palavra-passe")]
        public string NovaPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar nova palavra-passe")]
        [Compare("NovaPassword", ErrorMessage = "A confirmação não coincide com a nova palavra-passe.")]
        public string ConfirmarNovaPassword { get; set; } = string.Empty;
    }
}
