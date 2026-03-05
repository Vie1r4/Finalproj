using System.Text.Json;
using Finalproj.Data;
using Finalproj.Models;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Services;

public class LogSistemaService : ILogSistemaService
{
    private readonly FinalprojContext _context;

    public LogSistemaService(FinalprojContext context)
    {
        _context = context;
    }

    public async Task RegistarAsync(string acao, string? userId, string? userName, object? dados = null, CancellationToken cancellationToken = default)
    {
        var json = dados != null ? JsonSerializer.Serialize(dados) : null;
        _context.LogSistema.Add(new LogSistema
        {
            Acao = acao,
            UserId = userId,
            UserName = userName,
            JsonDados = json,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
