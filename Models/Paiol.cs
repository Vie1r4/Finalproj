using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models
{
    /// <summary>
    /// Entidade Paiol – contrato inicial (Tutorial Class 3): teto de segurança (NEM), perfil de risco e estado.
    /// "O Gestor escreve a lei para aquele espaço físico."
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

        /// <summary> Teto de Segurança (NEM) – "Aqui só entram X kg de pólvora". </summary>
        [Display(Name = "Teto de Segurança NEM (kg)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O limite deve ser um valor positivo.")]
        public decimal LimiteMLE { get; set; }

        /// <summary> Perfil de Risco (Licença) – "Este paiol só aceita material 1.3G" (ex.). </summary>
        [Required(ErrorMessage = "O perfil de risco é obrigatório.")]
        [StringLength(10)]
        [Display(Name = "Perfil de Risco")]
        public string PerfilRisco { get; set; } = string.Empty;

        /// <summary> Estado – Ativo (pode receber) ou Em Manutenção (bloqueado). </summary>
        [Required(ErrorMessage = "O estado é obrigatório.")]
        [StringLength(20)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = ConstantesPaiol.EstadoAtivo;
    }
}
