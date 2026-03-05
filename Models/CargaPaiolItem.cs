namespace Finalproj.Models;

/// <summary>
/// Item de carga (stock) num paiol para exibição. Ocupação baseada exclusivamente em NEM (legislação portuguesa).
/// A divisão serve para compatibilidade com o paiol; a NEM determina a capacidade ocupada.
/// </summary>
public class CargaPaiolItem
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = "";
    public decimal Quantidade { get; set; }
    public decimal NEMPorUnidade { get; set; }
    /// <summary> NEM total do item = Quantidade × NEM por unidade (kg). Conta para a ocupação do paiol. </summary>
    public decimal NEMTotal => Quantidade * NEMPorUnidade;
    /// <summary> Divisão de risco (ex.: 1.3G). Usada apenas para compatibilidade e relatórios, não para o cálculo da ocupação. </summary>
    public string Divisao { get; set; } = "";
}
