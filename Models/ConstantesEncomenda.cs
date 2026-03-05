namespace Finalproj.Models;

/// <summary>
/// Estados da encomenda conforme fluxo do ERP (Docs/Diagramas).
/// PENDENTE → ACEITE | REJEITADA; ACEITE → EM_PREPARACAO → CONCLUIDA.
/// </summary>
public static class ConstantesEncomenda
{
    public const string PENDENTE = "Pendente";
    public const string ACEITE = "Aceite";
    public const string REJEITADA = "Rejeitada";
    public const string EM_PREPARACAO = "Em preparação";
    public const string CONCLUIDA = "Concluída";

    /// <summary> Estados em que o stock continua reservado (não libertar para catálogo). </summary>
    public static readonly string[] EstadosComReserva = { PENDENTE, ACEITE, EM_PREPARACAO };

    public static bool TemReserva(string estado) =>
        EstadosComReserva.Contains(estado ?? "");

    public static string[] TodosEstados => new[] { PENDENTE, ACEITE, REJEITADA, EM_PREPARACAO, CONCLUIDA };
}
