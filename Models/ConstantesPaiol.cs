using Microsoft.AspNetCore.Mvc.Rendering;

namespace Finalproj.Models;

/// <summary>
/// Valores fixos do contrato do paiol (Class 3 – validações e opções de formulário).
/// </summary>
public static class ConstantesPaiol
{
    /// <summary> Licenças de paiol: 1.1 a 1.6. </summary>
    public static readonly string[] LicencasPaiol = { "1.1", "1.2", "1.3", "1.4", "1.5", "1.6" };

    /// <summary> Famílias de risco para produtos (inclui 1.4S). </summary>
    public static readonly string[] FamiliasRiscoProduto = { "1.1", "1.2", "1.3", "1.4", "1.4S", "1.5", "1.6" };

    /// <summary> Estados do paiol: Ativo pode receber carga; Em Manutenção bloqueado. </summary>
    public static readonly string[] Estados = { "Ativo", "Em Manutenção" };

    public const string EstadoAtivo = "Ativo";
    public const string EstadoEmManutencao = "Em Manutenção";

    public static readonly string[] PerfisRisco = LicencasPaiol;

    /// <summary> Cargos que podem ser atribuídos ao acesso a um paiol (Class 8 – roles). </summary>
    public static readonly string[] CargosDisponiveis = { "Admin", "Armazém", "Técnico", "Comercial" };

    /// <summary> Opções para dropdown de licença do paiol com texto em formato 1.3G, 1.4G, etc. </summary>
    public static List<SelectListItem> LicencasParaDropdown()
    {
        return LicencasPaiol.Select(v => new SelectListItem { Value = v, Text = v + "G" }).ToList();
    }

    /// <summary> Opções para dropdown de família de risco do produto (1.3G, 1.4G; 1.4S mantém-se). </summary>
    public static List<SelectListItem> FamiliasParaDropdown()
    {
        return FamiliasRiscoProduto.Select(v => new SelectListItem
        {
            Value = v,
            Text = v == "1.4S" ? v : v + "G"
        }).ToList();
    }
}
