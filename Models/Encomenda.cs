using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Encomenda do cliente. Estados: Pendente → Aceite | Rejeitada; Aceite → Em preparação → Concluída.
/// Stock reservado enquanto estado ∈ { Pendente, Aceite, Em preparação }.
/// </summary>
public class Encomenda
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Estado { get; set; } = ConstantesEncomenda.PENDENTE;

    [Display(Name = "Data de criação")]
    [DataType(DataType.DateTime)]
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    [Display(Name = "Data de conclusão")]
    [DataType(DataType.DateTime)]
    public DateTime? DataConclusao { get; set; }

    [StringLength(500)]
    [Display(Name = "Motivo de rejeição")]
    public string? MotivoRejeicao { get; set; }

    /// <summary> Utilizador (Identity) que aceitou a encomenda. </summary>
    [StringLength(450)]
    public string? FuncionarioAceiteUserId { get; set; }

    /// <summary> Utilizador (Identity) que preparou/concluiu a encomenda. </summary>
    [StringLength(450)]
    public string? FuncionarioPreparouUserId { get; set; }

    public ICollection<EncomendaItem> Itens { get; set; } = new List<EncomendaItem>();
}
