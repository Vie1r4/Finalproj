using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Uma linha de "retirar X do paiol Y" para a linha de encomenda Z.
/// </summary>
public class RetiradaPreparacaoInput
{
    public int EncomendaItemId { get; set; }
    public int PaiolId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "A quantidade não pode ser negativa.")]
    public decimal Quantidade { get; set; }
}
