namespace Finalproj.Models;

/// <summary>
/// Regras de compatibilidade entre licença do paiol e família de risco do produto (Class 3 – regras de negócio).
/// Baseado na norma: o que pode e não pode entrar em cada tipo de paiol.
/// </summary>
public static class RegrasLicencaPaiol
{
    /// <summary>
    /// Indica se um produto (família de risco) pode entrar num paiol com a dada licença.
    /// Aceita valores guardados como "1.3" ou "1.3G" (normaliza para 1.3).
    /// </summary>
    public static bool ProdutoPodeEntrar(string licencaPaiol, string familiaProduto)
    {
        if (string.IsNullOrWhiteSpace(licencaPaiol) || string.IsNullOrWhiteSpace(familiaProduto))
            return false;

        licencaPaiol = Normalizar(licencaPaiol);
        familiaProduto = Normalizar(familiaProduto);

        return licencaPaiol switch
        {
            "1.1" => Aceita(familiaProduto, "1.1", "1.2", "1.3", "1.4", "1.4S", "1.5", "1.6"),
            "1.2" => Aceita(familiaProduto, "1.2", "1.3", "1.4", "1.4S", "1.6"),
            "1.3" => Aceita(familiaProduto, "1.3", "1.4", "1.4S", "1.6"),
            "1.4" => Aceita(familiaProduto, "1.4", "1.4S", "1.6"),
            "1.5" => Aceita(familiaProduto, "1.1", "1.3", "1.4", "1.4S", "1.5"),
            "1.6" => Aceita(familiaProduto, "1.6"),
            _ => false
        };
    }

    private static string Normalizar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return valor;
        var t = valor.Trim();
        if (t.EndsWith("G", StringComparison.OrdinalIgnoreCase))
            return t[..^1].Trim();
        return t;
    }

    private static bool Aceita(string familia, params string[] permitidas)
    {
        return permitidas.Contains(familia);
    }

    /// <summary>
    /// Mensagem curta para mostrar ao utilizador quando a entrada é recusada por incompatibilidade.
    /// </summary>
    public static string MensagemRecusa(string licencaPaiol, string familiaProduto)
    {
        var licenca = Normalizar(licencaPaiol);
        var familia = Normalizar(familiaProduto);
        return licenca switch
        {
            "1.1" => "Paiol 1.1 (Bunker) aceita 1.1, 1.2, 1.3, 1.4, 1.5 e 1.6. O produto é " + familia + ".",
            "1.2" => "Paiol 1.2 não pode receber 1.1 nem 1.5 (risco de destruição). O produto é " + familia + ".",
            "1.3" => "Paiol 1.3 (incêndio violento) só aceita 1.3, 1.4 e 1.6. O produto é " + familia + ".",
            "1.4" => "Paiol 1.4 (risco reduzido) só aceita 1.4, 1.4S e 1.6. O produto é " + familia + ".",
            "1.5" => "Paiol 1.5 aceita 1.1, 1.3, 1.4 e 1.5. O produto é " + familia + ".",
            "1.6" => "Paiol 1.6 só aceita produtos 1.6. O produto é " + familia + ".",
            _ => "Este produto não tem autorização para entrar neste paiol (licença " + licenca + ", produto " + familia + ")."
        };
    }
}
