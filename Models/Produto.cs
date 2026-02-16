using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Produto com NEM por unidade e família de risco (Tutorial Class 3 – entidade com validações).
/// Usado no processo de decisão: "10 caixas × 5 kg = 50 kg de pólvora a entrar".
/// </summary>
public class Produto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome do produto é obrigatório.")]
    [StringLength(200)]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    /// <summary> NEM (kg de pólvora) por unidade – ex.: 5 kg por caixa. </summary>
    [Display(Name = "NEM por unidade (kg)")]
    [Range(0.0001, double.MaxValue, ErrorMessage = "O NEM por unidade deve ser positivo.")]
    public decimal NEMPorUnidade { get; set; }

    /// <summary> Classificação de risco – 1.1G, 1.3G, 1.4G (compatibilidade com Perfil do Paiol). </summary>
    [Required(ErrorMessage = "A classificação de risco é obrigatória.")]
    [StringLength(10)]
    [Display(Name = "Classificação de Risco")]
    public string FamiliaRisco { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Unidade")]
    public string? Unidade { get; set; }

    /// <summary> Filtro técnico do catálogo (Baterias, Bombas de Arremesso, Foguetes, etc.). </summary>
    [StringLength(30)]
    [Display(Name = "Filtro técnico")]
    public string? FiltroTecnico { get; set; }

    /// <summary> Calibre do catálogo (&lt; 20mm, 20–30mm, etc.). </summary>
    [StringLength(30)]
    [Display(Name = "Calibre")]
    public string? Calibre { get; set; }
}
