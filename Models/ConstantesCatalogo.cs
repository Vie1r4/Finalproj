using Microsoft.AspNetCore.Mvc.Rendering;

namespace Finalproj.Models;

/// <summary>
/// Valores fixos do catálogo: classificação de risco, filtro técnico e calibre (estrutura em níveis).
/// </summary>
public static class ConstantesCatalogo
{
    /// <summary> Classificação de risco (1.1G, 1.2G, …). Valores iguais aos de ConstantesPaiol.FamiliasRiscoProduto. </summary>
    public static readonly string[] ClassificacoesRisco = { "1.1", "1.2", "1.3", "1.4", "1.4S", "1.5", "1.6" };

    /// <summary> Texto para exibição: 1.4S mantém-se; restantes com sufixo G. </summary>
    public static string TextoClassificacao(string valor)
    {
        if (string.IsNullOrEmpty(valor)) return "";
        return valor == "1.4S" ? valor : valor + "G";
    }

    /// <summary> Filtros técnicos (tipo de material). Value = código guardado na BD. </summary>
    public static readonly (string Value, string Text)[] FiltrosTecnicos =
    {
        ("Baterias", "Baterias (Cakes)"),
        ("BombasArremesso", "Bombas de Arremesso (Shells)"),
        ("Foguetes", "Foguetes (Rockets)"),
        ("Candelas", "Candelas (Roman Candles)"),
        ("Monotiros", "Monotiros (Single Shots)"),
        ("GerbsVulcoes", "Gerbs/Vulcões")
    };

    /// <summary> Calibres. Value = código guardado na BD. </summary>
    public static readonly (string Value, string Text)[] Calibres =
    {
        ("MuitoPequeno", "< 20mm (Muito pequeno)"),
        ("BateriasPadrao", "20mm - 30mm (Baterias padrão)"),
        ("BombasPequenas", "50mm - 75mm (Bombas pequenas)"),
        ("BombasMedias", "100mm - 125mm (Bombas médias - 4\" a 5\")"),
        ("BombasGrandes", "> 150mm (Bombas grandes - 6\"+)")
    };

    public static List<SelectListItem> FiltrosTecnicosParaDropdown()
    {
        return FiltrosTecnicos
            .Select(x => new SelectListItem { Value = x.Value, Text = x.Text })
            .ToList();
    }

    public static List<SelectListItem> CalibresParaDropdown()
    {
        return Calibres
            .Select(x => new SelectListItem { Value = x.Value, Text = x.Text })
            .ToList();
    }

    public static string TextoFiltroTecnico(string? valor)
    {
        if (string.IsNullOrEmpty(valor)) return "—";
        var item = FiltrosTecnicos.FirstOrDefault(f => f.Value == valor);
        return item.Text;
    }

    public static string TextoCalibre(string? valor)
    {
        if (string.IsNullOrEmpty(valor)) return "—";
        var item = Calibres.FirstOrDefault(c => c.Value == valor);
        return item.Text;
    }
}
