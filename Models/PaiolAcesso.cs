using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Cargos (roles) que podem aceder a um paiol (Class 4 – FK ao Paiol; Class 8 – roles).
/// Definido ao criar/editar o paiol; usado para filtrar quem vê o paiol em "Registar entrada".
/// </summary>
public class PaiolAcesso
{
    public int Id { get; set; }

    public int PaiolId { get; set; }
    public Paiol Paiol { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = string.Empty;
}
