namespace Finalproj.Models;

/// <summary>
/// Resultado da validação do motor de entradas no paiol (PROMPT_Motor_Validacao_Paiol).
/// Nunca lança exceções — devolve sempre um resultado com aprovado, erros e avisos.
/// </summary>
public class ResultadoValidacaoPaiol
{
    public bool Aprovado { get; set; }
    public List<ErroValidacao> Erros { get; set; } = new();
    public List<AvisoValidacao> Avisos { get; set; } = new();
    public decimal? OcupacaoResultantePercentagem { get; set; }
    public string? DivisaoDominanteResultante { get; set; }
}

public class ErroValidacao
{
    public string Codigo { get; set; } = "";
    public string Mensagem { get; set; } = "";
}

public class AvisoValidacao
{
    public string Codigo { get; set; } = "";
    public string Mensagem { get; set; } = "";
}
