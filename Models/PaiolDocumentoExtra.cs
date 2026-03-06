using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Documento do paiol (licença, planta, etc.) com nome à escolha.
/// </summary>
public class PaiolDocumentoExtra
{
    public int Id { get; set; }

    public int PaiolId { get; set; }
    public Paiol Paiol { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [Display(Name = "Nome do documento")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Caminho { get; set; } = string.Empty;
}
