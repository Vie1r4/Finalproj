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

        /// <summary> Tipo de paiol (PERMANENTE_GERAL, PROVISORIO_EVENTO, etc.) para Regra 9 do motor. </summary>
        [StringLength(30)]
        [Display(Name = "Tipo de paiol")]
        public string? TipoPaiol { get; set; }

        /// <summary> Data de validade da licença PSP. Se expirada, bloqueia entradas (Regra 1). </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Validade da licença")]
        public DateTime? DataValidadeLicenca { get; set; }

        /// <summary> Número da licença PSP (referência). </summary>
        [StringLength(50)]
        [Display(Name = "N.º licença")]
        public string? NumeroLicenca { get; set; }

        /// <summary> Divisões autorizadas na licença (ex.: "1.3,1.4,1.4S"). Vazio = usa Perfil de Risco (legado). </summary>
        [StringLength(100)]
        [Display(Name = "Divisões autorizadas (licença)")]
        public string? DivisoesAutorizadas { get; set; }

        /// <summary> Grupos autorizados na licença (ex.: "G,S"). Vazio = aceita todos (legado). </summary>
        [StringLength(50)]
        [Display(Name = "Grupos autorizados (licença)")]
        public string? GruposAutorizados { get; set; }

        /// <summary> Divisão mais perigosa atualmente no paiol (atualizada pela Regra 5 do motor). </summary>
        [StringLength(10)]
        public string? DivisaoDominante { get; set; }

        /// <summary> Início do período licenciado (paióis provisórios). </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data início (provisório)")]
        public DateTime? DataInicio { get; set; }

        /// <summary> Fim do período licenciado (paióis provisórios). </summary>
        [DataType(DataType.Date)]
        [Display(Name = "Data fim (provisório)")]
        public DateTime? DataFim { get; set; }
    }
}
