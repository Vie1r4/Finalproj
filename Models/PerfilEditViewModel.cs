using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models
{
    /// <summary>
    /// Dados exibidos e editáveis na página de perfil. Email e UserName são só leitura.
    /// </summary>
    public class PerfilEditViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [StringLength(50)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        /// <summary> Cargo(s) do utilizador (Class 8 – Roles). Só leitura. </summary>
        public IList<string> Roles { get; set; } = new List<string>();

        /// <summary> Data de criação do perfil / registo. Só leitura. </summary>
        public DateTime? DataRegisto { get; set; }
    }
}
