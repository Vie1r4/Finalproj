namespace Finalproj.Models;

/// <summary>
/// Paiol com dados de ocupação (MLE atual e percentagem do teto de segurança) para listagens.
/// </summary>
public class PaiolComOcupacaoViewModel
{
    public Paiol Paiol { get; set; } = null!;
    /// <summary> MLE atual no paiol (kg). </summary>
    public decimal MleAtual { get; set; }
    /// <summary> Percentagem de ocupação (0–100+). Pode exceder 100 se houver sobrecarga. </summary>
    public decimal PercentagemOcupacao { get; set; }
}
