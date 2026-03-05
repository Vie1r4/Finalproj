using Microsoft.AspNetCore.Mvc.Rendering;

namespace Finalproj.Models;

/// <summary>
/// Valores fixos para formulários de funcionários e clientes.
/// </summary>
public static class ConstantesFuncionariosClientes
{
    /// <summary> Cargos disponíveis (alinhado aos roles do sistema). </summary>
    public static readonly string[] Cargos = { "Admin", "Armazém", "Técnico", "Comercial" };

    public static List<SelectListItem> CargosParaDropdown()
    {
        return Cargos.Select(c => new SelectListItem { Value = c, Text = c }).ToList();
    }

    /// <summary> Tipos de cliente. </summary>
    public static readonly (string Value, string Text)[] TiposCliente =
    {
        ("Particular", "Particular"),
        ("Empresa", "Empresa")
    };

    public static List<SelectListItem> TiposClienteParaDropdown()
    {
        return TiposCliente.Select(t => new SelectListItem { Value = t.Value, Text = t.Text }).ToList();
    }
}
