using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Ficha de funcionário. Campos alinhados a gestão de pessoal e RGPD.
/// Pode estar associado a um utilizador (UserId) quando tem acesso ao sistema.
/// </summary>
public class Funcionario : IValidatableObject
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

    [Display(Name = "Data de nascimento")]
    [DataType(DataType.Date)]
    public DateTime? DataNascimento { get; set; }

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

    [StringLength(10)]
    [Display(Name = "Código-postal")]
    public string? CodigoPostal { get; set; }

    [StringLength(100)]
    [Display(Name = "Localidade")]
    public string? Localidade { get; set; }

    [StringLength(50)]
    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }

    [Display(Name = "Data de admissão")]
    [DataType(DataType.Date)]
    public DateTime? DataAdmissao { get; set; }

    [Display(Name = "Data de saída")]
    [DataType(DataType.Date)]
    public DateTime? DataSaida { get; set; }

    [StringLength(12)]
    [Display(Name = "N.º Segurança Social")]
    public string? NumeroSegurancaSocial { get; set; }

    [StringLength(34)]
    [Display(Name = "IBAN")]
    public string? IBAN { get; set; }

    [StringLength(500)]
    [Display(Name = "Notas")]
    public string? Notas { get; set; }

    [StringLength(450)]
    public string? UserId { get; set; }

    [Display(Name = "Data de registo")]
    [DataType(DataType.DateTime)]
    public DateTime? DataRegisto { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataAdmissao.HasValue && DataSaida.HasValue && DataSaida.Value < DataAdmissao.Value)
            yield return new ValidationResult("A data de saída não pode ser anterior à data de admissão.", new[] { nameof(DataSaida) });
    }
}
