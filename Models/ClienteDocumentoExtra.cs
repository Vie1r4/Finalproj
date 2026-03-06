using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Documento do cliente com nome à escolha (ex.: contrato, certidão). Permite vários documentos por cliente.
/// </summary>
public class ClienteDocumentoExtra
{
    public int Id { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    [Required]
    [StringLength(100)]
    [Display(Name = "Nome do documento")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Caminho { get; set; } = string.Empty;
}
