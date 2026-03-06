using System.Globalization;

namespace Finalproj.Models;

/// <summary>
/// Motor de validação de entradas em paióis pirotécnicos (PROMPT_Motor_Validacao_Paiol).
/// Executa as 9 regras em sequência; se uma falhar, bloqueia e devolve o erro. Nunca lança exceções.
/// Base: ADR Classe 1 e RFACEPE.
/// </summary>
public static class MotorValidacaoPaiol
{
    private const decimal LimiteMleGrupoCComG = 50m;

    /// <summary> Hierarquia de perigo (do mais para o menos perigoso). </summary>
    private static readonly string[] HierarquiaDivisao = { "1.1", "1.2", "1.3", "1.4", "1.4S" };

    /// <summary>
    /// Valida se a entrada do produto no paiol pode ser aceite.
    /// </summary>
    /// <param name="dataValidadeLote">Opcional. Data de validade do lote a entrar; se fornecida e expirada, bloqueia (Regra 8). O catálogo não tem validade — a validade é do lote físico.</param>
    public static ResultadoValidacaoPaiol ValidarEntrada(
        Produto produto,
        Paiol paiol,
        decimal quantidadeEntrada,
        IReadOnlyList<ProdutoNoPaiolDto> produtosNoPaiol,
        decimal mleAtualPaiol,
        DateTime? dataValidadeLote = null)
    {
        var r = new ResultadoValidacaoPaiol();
        var divisaoProduto = NormalizarDivisao(produto.FamiliaRisco);
        var grupoProduto = (produto.GrupoCompatibilidade ?? "G").Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(grupoProduto)) grupoProduto = "G";
        var mleEntrada = quantidadeEntrada * produto.NEMPorUnidade;
        var lotacaoMax = paiol.LimiteMLE;

        // REGRA 1 — Licença do paiol válida
        if (paiol.DataValidadeLicenca.HasValue && paiol.DataValidadeLicenca.Value.Date < DateTime.UtcNow.Date)
        {
            r.Erros.Add(new ErroValidacao
            {
                Codigo = "ERRO_001",
                Mensagem = "Licença do paiol expirada. Renovação obrigatória antes de qualquer entrada de stock."
            });
            return r;
        }

        // REGRA 2 — Divisão autorizada pela licença (usa Perfil de Risco do paiol)
        if (!RegrasLicencaPaiol.ProdutoPodeEntrar(paiol.PerfilRisco, produto.FamiliaRisco))
        {
            r.Erros.Add(new ErroValidacao
            {
                Codigo = "ERRO_002",
                Mensagem = RegrasLicencaPaiol.MensagemRecusa(paiol.PerfilRisco, produto.FamiliaRisco)
            });
            return r;
        }

        // REGRA 3 — Grupo autorizado (removido: já não há GruposAutorizados no paiol; compatibilidade entre produtos continua na Regra 4) — Compatibilidade com produtos já existentes (matriz ADR 7.2.5)
        foreach (var q in produtosNoPaiol)
        {
            var grupoExistente = (q.Grupo ?? "G").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(grupoExistente)) grupoExistente = "G";
            if (!GruposCompativel(grupoExistente, grupoProduto))
            {
                r.Erros.Add(new ErroValidacao
                {
                    Codigo = "ERRO_004",
                    Mensagem = $"Incompatibilidade de grupos: produto do grupo {grupoProduto} não pode coexistir com produto do grupo {grupoExistente} já existente no paiol (ADR 7.2.5)."
                });
                return r;
            }
        }

        // Sub-regra 4a: C e G juntos — só se MLE grupo C no paiol < 50 kg
        var somaMleGrupoC = produtosNoPaiol.Where(p => (p.Grupo ?? "G").Trim().ToUpperInvariant() == "C").Sum(p => p.MLE);
        if (grupoProduto == "G" && somaMleGrupoC >= LimiteMleGrupoCComG)
        {
            r.Erros.Add(new ErroValidacao
            {
                Codigo = "ERRO_004",
                Mensagem = "Incompatibilidade: o paiol já contém 50 kg MLE ou mais de grupo C (pólvoras). Não pode adicionar produtos do grupo G (sub-regra 4a ADR)."
            });
            return r;
        }
        if (grupoProduto == "C")
        {
            if (somaMleGrupoC + mleEntrada > LimiteMleGrupoCComG && produtosNoPaiol.Any(p => (p.Grupo ?? "G").Trim().ToUpperInvariant() == "G"))
            {
                r.Erros.Add(new ErroValidacao
                {
                    Codigo = "ERRO_004",
                    Mensagem = "Incompatibilidade: grupo C (pólvoras) com grupo G no mesmo paiol só é permitido até 50 kg MLE de C. Sub-regra 4a ADR."
                });
                return r;
            }
        }

        // REGRA 5 — Divisão mais perigosa (atualizar estado; nunca bloqueia)
        var divisoesApos = produtosNoPaiol.Select(p => NormalizarDivisao(p.Divisao)).ToList();
        divisoesApos.Add(divisaoProduto);
        r.DivisaoDominanteResultante = ObterDivisaoMaisPerigosa(divisoesApos);

        // REGRA 6 — Lotação máxima
        var mleTotalApos = mleAtualPaiol + mleEntrada;
        if (mleTotalApos > lotacaoMax)
        {
            var pct = lotacaoMax > 0 ? (mleTotalApos / lotacaoMax * 100).ToString("N1", CultureInfo.InvariantCulture) : "—";
            r.Erros.Add(new ErroValidacao
            {
                Codigo = "ERRO_005",
                Mensagem = $"Lotação máxima excedida. Lotação autorizada: {lotacaoMax:N2} kg MLE. Ocupação após entrada: {mleTotalApos:N2} kg MLE ({pct}%)."
            });
            return r;
        }

        // REGRA 7 — Alertas de ocupação (não bloqueia)
        if (lotacaoMax > 0)
        {
            var percentagem = (mleTotalApos / lotacaoMax) * 100;
            r.OcupacaoResultantePercentagem = percentagem;
            if (percentagem >= 90 && percentagem < 100)
                r.Avisos.Add(new AvisoValidacao { Codigo = "AVISO_003", Mensagem = "Paiol quase cheio (90% ou mais). Evite programar novas entradas." });
            else if (percentagem >= 80 && percentagem < 90)
                r.Avisos.Add(new AvisoValidacao { Codigo = "AVISO_002", Mensagem = "Paiol em zona de atenção (80% ou mais)." });
        }

        // Aviso C+G juntos (sub-regra 4a)
        if (grupoProduto == "G" && somaMleGrupoC > 0 && somaMleGrupoC < LimiteMleGrupoCComG)
            r.Avisos.Add(new AvisoValidacao { Codigo = "AVISO_001", Mensagem = "Grupos C e G juntos no paiol. Atenção ao limite de 50 kg MLE de grupo C (ADR sub-regra 4a)." });
        if (grupoProduto == "C" && produtosNoPaiol.Any(p => (p.Grupo ?? "G").Trim().ToUpperInvariant() == "G"))
            r.Avisos.Add(new AvisoValidacao { Codigo = "AVISO_001", Mensagem = "Grupos C e G juntos no paiol. Atenção ao limite de 50 kg MLE de grupo C." });

        // REGRA 8 — Validade do lote (quando informada na entrada). O catálogo não tem validade; o lote físico pode ter.
        if (dataValidadeLote.HasValue && dataValidadeLote.Value.Date < DateTime.UtcNow.Date)
        {
            r.Erros.Add(new ErroValidacao
            {
                Codigo = "ERRO_006",
                Mensagem = $"Lote fora de prazo de validade. Data de validade: {dataValidadeLote:dd/MM/yyyy}. Não pode entrar em stock ativo."
            });
            return r;
        }

        r.Aprovado = true;
        return r;
    }

    private static bool GruposCompativel(string grupoExistente, string grupoNovo)
    {
        // Matriz ADR 7.2.5: B-C-D-G-S (B só B; C com C,G,S; D só D; G com C,G,S; S com C,G,S)
        var matriz = new Dictionary<string, Dictionary<string, bool>>
        {
            ["B"] = new() { ["B"] = true, ["C"] = false, ["D"] = false, ["G"] = false, ["S"] = false },
            ["C"] = new() { ["B"] = false, ["C"] = true, ["D"] = false, ["G"] = true, ["S"] = true },
            ["D"] = new() { ["B"] = false, ["C"] = false, ["D"] = true, ["G"] = false, ["S"] = false },
            ["G"] = new() { ["B"] = false, ["C"] = true, ["D"] = false, ["G"] = true, ["S"] = true },
            ["S"] = new() { ["B"] = false, ["C"] = true, ["D"] = false, ["G"] = true, ["S"] = true }
        };
        var ge = grupoExistente.Length > 0 ? grupoExistente[0].ToString() : "G";
        var gn = grupoNovo.Length > 0 ? grupoNovo[0].ToString() : "G";
        if (!matriz.ContainsKey(ge) || !matriz[ge].ContainsKey(gn))
            return true;
        return matriz[ge][gn];
    }

    private static string NormalizarDivisao(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return valor ?? "";
        var t = valor.Trim();
        if (t.EndsWith("G", StringComparison.OrdinalIgnoreCase))
            return t[..^1].Trim();
        return t;
    }

    private static string ObterDivisaoMaisPerigosa(List<string> divisoes)
    {
        foreach (var d in HierarquiaDivisao)
        {
            if (divisoes.Contains(d))
                return d;
        }
        return divisoes.FirstOrDefault() ?? "";
    }

    private static List<string> ObterLista(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return new List<string>();
        return valor.Split(',', ';').Select(s => s.Trim().ToUpperInvariant()).Where(s => s.Length > 0).ToList();
    }

    private static List<string> ObterListaDivisoes(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return new List<string>();
        return valor.Split(',', ';').Select(s => NormalizarDivisao(s.Trim())).Where(s => s.Length > 0).Distinct().ToList();
    }
}
