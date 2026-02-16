namespace Finalproj.Models;

/// <summary>
/// Item de carga (stock) num paiol para exibição em Detalhes: produto, quantidade efetiva e NEM.
/// </summary>
public class CargaPaiolItem
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";
    public decimal Quantidade { get; set; }
    public decimal NEMPorUnidade { get; set; }
    public decimal NEMTotal => Quantidade * NEMPorUnidade;
}
