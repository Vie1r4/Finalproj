using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models
{
    /// <summary>
    /// Tutorial Class 8: "Add other information to the User. If we need to store any other information
    /// of the user profile, we can create another model with the new data. Create a new model Perfil.cs"
    /// </summary>
    public class Perfil
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Nome")]
        public string? Nome { get; set; }

        [StringLength(50)]
        [Display(Name = "Telefone")]
        public string? Telefone { get; set; }

        [Display(Name = "Data de registo")]
        public DateTime? DataRegisto { get; set; }
    }
}
