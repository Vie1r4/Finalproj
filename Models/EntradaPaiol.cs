using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Registo de entrada de produto num paiol (Class 4 – FK e navegação).
/// Usado para somar o NEM atual no paiol e validar novas entradas.
/// </summary>
public class EntradaPaiol
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

    [Display(Name = "Data de entrada")]
    public DateTime DataEntrada { get; set; } = DateTime.UtcNow;
}
