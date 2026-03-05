using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Registo imutável de auditoria do sistema (fluxo ERP).
/// Todas as ações relevantes (entrada/saída de stock, aceite/rejeição/conclusão de encomenda) geram um log.
/// </summary>
public class LogSistema
{
    public long Id { get; set; }

    [Required]
    [StringLength(80)]
    public string Acao { get; set; } = string.Empty;

    [StringLength(450)]
    public string? UserId { get; set; }

    [StringLength(200)]
    public string? UserName { get; set; }

    /// <summary> Dados estruturados em JSON (IDs, quantidades, códigos). </summary>
    public string? JsonDados { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
