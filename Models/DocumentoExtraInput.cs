namespace Finalproj.Models;

/// <summary>
/// Entrada de um documento adicional no Create/Edit do funcionário (nome + ficheiro).
/// </summary>
public class DocumentoExtraInput
{
    public string? Nome { get; set; }
    public IFormFile? Ficheiro { get; set; }
}
