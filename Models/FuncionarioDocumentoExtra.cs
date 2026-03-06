using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Documento adicional do funcionário com nome personalizado (ex.: "Certificado X", "Contrato").
/// Permite vários "outros" documentos por funcionário.
/// </summary>
public class FuncionarioDocumentoExtra
{
    public int Id { get; set; }

    public int FuncionarioId { get; set; }
    public Funcionario Funcionario { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [Display(Name = "Nome do documento")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Caminho { get; set; } = string.Empty;
}
