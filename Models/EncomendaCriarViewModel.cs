using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Linha para criar uma encomenda (produto + quantidade).
/// </summary>
public class EncomendaItemCriarViewModel
{
    public int ProdutoId { get; set; }
    public string ProdutoNome { get; set; } = string.Empty;
    public decimal StockDisponivel { get; set; }

    [Range(0.0001, double.MaxValue, ErrorMessage = "A quantidade deve ser positiva.")]
    public decimal Quantidade { get; set; }
}

/// <summary>
/// ViewModel para a página de criação de encomenda.
/// </summary>
public class EncomendaCriarViewModel
{
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    public List<EncomendaItemCriarViewModel> Itens { get; set; } = new();
}

/// <summary>
/// Rascunho da encomenda guardado em sessão (cliente + itens adicionados).
/// </summary>
public class EncomendaDraftViewModel
{
    public int ClienteId { get; set; }
    public List<EncomendaItemCriarViewModel> Itens { get; set; } = new();
}
