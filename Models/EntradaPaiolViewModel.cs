using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// ViewModel para o formulário de entrada de produto no paiol (processo de decisão).
/// </summary>
public class EntradaPaiolViewModel
{
    [Display(Name = "Paiol")]
    public int PaiolId { get; set; }

    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "A quantidade deve ser positiva.")]
    [Display(Name = "Quantidade")]
    public decimal Quantidade { get; set; }

    [StringLength(50)]
    [Display(Name = "N.º de lote")]
    public string? NumeroLote { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de fabrico")]
    public DateTime? DataFabrico { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Data de validade")]
    public DateTime? DataValidade { get; set; }
}
