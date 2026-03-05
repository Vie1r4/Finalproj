namespace Finalproj.Models;

/// <summary>
/// Regras de compatibilidade entre licença do paiol e classificação de risco do produto (Class 3 – regras de negócio).
/// Alinhado à matriz ADR 7.5.2.2 (documento Funcionalidades_Requisitos_Legais_Pirotecnia): co-armazenamento por divisão.
/// </summary>
public static class RegrasLicencaPaiol
{
    /// <summary>
    /// Indica se um produto (divisão de risco) pode entrar num paiol com a dada licença (perfil de risco).
    /// Aceita valores guardados como "1.3" ou "1.3G" (normaliza para 1.3). Matriz ADR: 1.1 só com 1.1; 1.2 só com 1.2; 1.3/1.4/1.4S entre si.
    /// </summary>
    public static bool ProdutoPodeEntrar(string licencaPaiol, string familiaProduto)
    {
        if (string.IsNullOrWhiteSpace(licencaPaiol) || string.IsNullOrWhiteSpace(familiaProduto))
            return false;

        licencaPaiol = Normalizar(licencaPaiol);
        familiaProduto = Normalizar(familiaProduto);

        return licencaPaiol switch
        {
            "1.1" => Aceita(familiaProduto, "1.1"),
            "1.2" => Aceita(familiaProduto, "1.2"),
            "1.3" => Aceita(familiaProduto, "1.3", "1.4", "1.4S"),
            "1.4" => Aceita(familiaProduto, "1.3", "1.4", "1.4S"),
            "1.4S" => Aceita(familiaProduto, "1.3", "1.4", "1.4S"),
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
    /// Mensagem para o utilizador quando a entrada é recusada por incompatibilidade (ADR 7.5.2.2).
    /// </summary>
    public static string MensagemRecusa(string licencaPaiol, string familiaProduto)
    {
        var licenca = Normalizar(licencaPaiol);
        var familia = Normalizar(familiaProduto);
        return licenca switch
        {
            "1.1" => "Paiol 1.1G só pode conter produtos 1.1G (ADR 7.5.2.2). O produto é " + familia + ".",
            "1.2" => "Paiol 1.2G só pode conter produtos 1.2G (ADR 7.5.2.2). O produto é " + familia + ".",
            "1.3" => "Paiol 1.3G aceita apenas produtos 1.3G, 1.4G ou 1.4S. O produto é " + familia + ".",
            "1.4" => "Paiol 1.4G aceita apenas produtos 1.3G, 1.4G ou 1.4S. O produto é " + familia + ".",
            "1.4S" => "Paiol 1.4S aceita apenas produtos 1.3G, 1.4G ou 1.4S. O produto é " + familia + ".",
            "1.5" => "Paiol 1.5 aceita 1.1, 1.3, 1.4, 1.4S e 1.5. O produto é " + familia + ".",
            "1.6" => "Paiol 1.6 só aceita produtos 1.6. O produto é " + familia + ".",
            _ => "Este produto não tem autorização para entrar neste paiol (licença " + licenca + ", produto " + familia + ")."
        };
    }
}
