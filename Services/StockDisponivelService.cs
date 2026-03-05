using Finalproj.Data;
using Finalproj.Models;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Services;

/// <summary>
/// Cálculo de stock disponível por produto (entradas - saídas - reservas ativas).
/// Alinhado ao fluxo ERP: stock reservado não conta como disponível para o catálogo.
/// </summary>
public static class StockDisponivelService
{
    /// <summary>
    /// Devolve a quantidade disponível (kg) por produto: soma entradas - soma saídas - soma reservas (encomendas em Pendente/Aceite/Em preparação).
    /// </summary>
    public static async Task<Dictionary<int, decimal>> ObterStockDisponivelPorProdutoAsync(
        FinalprojContext context,
        CancellationToken cancellationToken = default)
    {
        var entradas = await context.EntradasPaiol
            .GroupBy(e => e.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Total = g.Sum(e => e.Quantidade) })
            .ToListAsync(cancellationToken);

        var saidas = await context.SaidasPaiol
            .GroupBy(s => s.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Total = g.Sum(s => s.Quantidade) })
            .ToListAsync(cancellationToken);

        var reservas = await context.Reservas
            .Include(r => r.Encomenda)
            .Where(r => ConstantesEncomenda.EstadosComReserva.Contains(r.Encomenda.Estado))
            .GroupBy(r => r.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Total = g.Sum(r => r.Quantidade) })
            .ToListAsync(cancellationToken);

        var resultado = new Dictionary<int, decimal>();
        foreach (var e in entradas)
            resultado[e.ProdutoId] = e.Total;
        foreach (var s in saidas)
            resultado[s.ProdutoId] = resultado.GetValueOrDefault(s.ProdutoId) - s.Total;
        foreach (var r in reservas)
            resultado[r.ProdutoId] = resultado.GetValueOrDefault(r.ProdutoId) - r.Total;

        // Stock disponível não pode ser negativo: valores negativos indicam reservas/saídas em excesso e mostram-se como 0
        var chaves = resultado.Keys.ToList();
        foreach (var pid in chaves)
        {
            if (resultado[pid] < 0)
                resultado[pid] = 0;
        }

        return resultado;
    }

    /// <summary>
    /// Quantidade disponível para um produto (0 se não houver stock).
    /// </summary>
    public static async Task<decimal> ObterStockDisponivelAsync(FinalprojContext context, int produtoId, CancellationToken cancellationToken = default)
    {
        var porProduto = await ObterStockDisponivelPorProdutoAsync(context, cancellationToken);
        return porProduto.GetValueOrDefault(produtoId, 0);
    }
}
