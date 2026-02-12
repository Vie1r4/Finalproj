using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models
{
    /// <summary>
    /// Entidade Paiol - armazém com limites físicos e legais (proposta PIROFAFE).
    /// </summary>
    public class Paiol
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do paiol é obrigatório.")]
        [StringLength(200)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Localização")]
        public string? Localizacao { get; set; }

        [Display(Name = "Limite MLE (kg)")]
        [Range(0, double.MaxValue, ErrorMessage = "O limite deve ser um valor positivo.")]
        public decimal LimiteMLE { get; set; }
    }
}
