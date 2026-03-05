namespace Finalproj.Models;

/// <summary>
/// Dados de um produto atualmente no paiol (para o motor de validação).
/// </summary>
public class ProdutoNoPaiolDto
{
    public string Divisao { get; set; } = "";
    public string Grupo { get; set; } = "G";
    public decimal Quantidade { get; set; }
    public decimal NEMPorUnidade { get; set; }
    public decimal MLE => Quantidade * NEMPorUnidade;
}
