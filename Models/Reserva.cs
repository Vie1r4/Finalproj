using System.ComponentModel.DataAnnotations;

namespace Finalproj.Models;

/// <summary>
/// Reserva de stock por produto para uma encomenda (Pendente / Aceite / Em preparação).
/// O stock disponível no catálogo = entradas - saídas - sum(reservas com estado em EstadosComReserva).
/// Ao rejeitar a encomenda, as reservas são removidas e o stock volta a ficar disponível.
/// </summary>
public class Reserva
{
    public int Id { get; set; }

    public int EncomendaId { get; set; }
    public Encomenda Encomenda { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    [Range(0.0001, double.MaxValue)]
    public decimal Quantidade { get; set; }
}
