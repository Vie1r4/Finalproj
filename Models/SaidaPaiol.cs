using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Registo de saída de produto de um paiol (Class 4 – FK e navegação).
/// O stock efetivo é (entradas − saídas) por produto no paiol.
/// </summary>
public class SaidaPaiol
{
    public int Id { get; set; }

    public int PaiolId { get; set; }
    [Display(Name = "Paiol")]
    public Paiol Paiol { get; set; } = null!;

    public int ProdutoId { get; set; }
    [Display(Name = "Produto")]
    public Produto Produto { get; set; } = null!;

    [Range(0.0001, double.MaxValue, ErrorMessage = "A quantidade deve ser positiva.")]
    public decimal Quantidade { get; set; }

    [Display(Name = "Data de saída")]
    public DateTime DataSaida { get; set; } = DateTime.UtcNow;

    /// <summary> Utilizador (Identity) que efetuou a retirada. </summary>
    [StringLength(450)]
    [Display(Name = "Retirado por")]
    public string? FuncionarioRetirouUserId { get; set; }

    /// <summary> Encomenda à qual esta saída está associada (quando aplicável). </summary>
    public int? EncomendaId { get; set; }

    /// <summary> Lote (entrada) de origem para rastreabilidade e FIFO. </summary>
    public int? EntradaPaiolId { get; set; }
    public EntradaPaiol? EntradaPaiol { get; set; }
}
