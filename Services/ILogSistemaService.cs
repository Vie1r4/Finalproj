namespace Finalproj.Services;

/// <summary>
/// Registo imutável de ações no sistema (entrada/saída stock, encomenda aceite/rejeitada/concluída).
/// </summary>
public interface ILogSistemaService
{
    Task RegistarAsync(string acao, string? userId, string? userName, object? dados = null, CancellationToken cancellationToken = default);
}
