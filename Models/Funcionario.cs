using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Ficha de funcionário. Campos alinhados a gestão de pessoal e RGPD.
/// Pode estar associado a um utilizador (UserId) quando tem acesso ao sistema.
/// Documentos: cartão de cidadão e ADDR (se aplicável) guardados em ficheiro.
/// </summary>
public class Funcionario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome completo é obrigatório.")]
    [StringLength(200)]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [StringLength(9, MinimumLength = 9, ErrorMessage = "O NIF deve ter 9 dígitos.")]
    [Display(Name = "NIF")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "NIF inválido (apenas 9 dígitos).")]
    public string? NIF { get; set; }

    [EmailAddress(ErrorMessage = "Email inválido.")]
    [StringLength(256)]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [StringLength(20)]
    [Display(Name = "Telefone")]
    [Phone(ErrorMessage = "Telefone inválido.")]
    public string? Telefone { get; set; }

    [StringLength(300)]
    [Display(Name = "Morada")]
    public string? Morada { get; set; }

    [StringLength(12)]
    [Display(Name = "N.º Segurança Social")]
    public string? NumeroSegurancaSocial { get; set; }

    [StringLength(34)]
    [Display(Name = "IBAN")]
    public string? IBAN { get; set; }

    [StringLength(50)]
    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }

    [StringLength(500)]
    [Display(Name = "Notas")]
    public string? Notas { get; set; }

    /// <summary> Associado a um utilizador do sistema (tem conta de acesso). </summary>
    [StringLength(450)]
    public string? UserId { get; set; }

    [Display(Name = "Data de registo")]
    [DataType(DataType.DateTime)]
    public DateTime? DataRegisto { get; set; }

    // --- Documentos: Cartão de Cidadão, ADR, Licença de Operador ---

    [StringLength(500)]
    [Display(Name = "Cartão de cidadão")]
    public string? CartaoCidadaoCaminho { get; set; }

    [StringLength(500)]
    [Display(Name = "ADR")]
    public string? DocumentoADDRCaminho { get; set; }

    [StringLength(500)]
    [Display(Name = "Licença de Operador")]
    public string? LicencaOperadorCaminho { get; set; }

    [StringLength(500)]
    [Display(Name = "Outros")]
    public string? OutrosCaminho { get; set; }

    /// <summary> Documentos adicionais com nome personalizado (substitui o uso de um único "Outros"). </summary>
    public ICollection<FuncionarioDocumentoExtra> DocumentosExtras { get; set; } = new List<FuncionarioDocumentoExtra>();
}
