using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// ViewModel para o formulário de saída de produto do paiol.
/// </summary>
public class SaidaPaiolViewModel
{
    [Display(Name = "Paiol")]
    public int PaiolId { get; set; }

    [Display(Name = "Produto")]
    public int ProdutoId { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "A quantidade deve ser positiva.")]
    [Display(Name = "Quantidade")]
    public decimal Quantidade { get; set; }
}
