using Finalproj.Data;
using Finalproj.Models;
using Finalproj.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Finalproj.Controllers;

/// <summary>
/// Fluxo de encomendas conforme diagrama ERP: Pendente → Aceite | Rejeitada; Aceite → Em preparação → Concluída.
/// Stock reservado enquanto estado ∈ { Pendente, Aceite, Em preparação }; libertado ao rejeitar ou ao concluir (saídas já registadas).
/// </summary>
[Authorize]
public class EncomendasController : Controller
{
    private const string SessionKeyDraft = "EncomendaDraft";
    private readonly FinalprojContext _context;
    private readonly ILogSistemaService _logSistema;
    private readonly UserManager<IdentityUser> _userManager;

    public EncomendasController(FinalprojContext context, ILogSistemaService logSistema, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _logSistema = logSistema;
        _userManager = userManager;
    }

    private EncomendaDraftViewModel? GetDraft()
    {
        var json = HttpContext.Session.GetString(SessionKeyDraft);
        if (string.IsNullOrEmpty(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<EncomendaDraftViewModel>(json);
        }
        catch { return null; }
    }

    private void SetDraft(EncomendaDraftViewModel draft)
    {
        HttpContext.Session.SetString(SessionKeyDraft, JsonSerializer.Serialize(draft));
    }

    private void ClearDraft()
    {
        HttpContext.Session.Remove(SessionKeyDraft);
    }

    public async Task<IActionResult> Index(string? estado, CancellationToken cancellationToken = default)
    {
        IQueryable<Encomenda> query = _context.Encomendas
            .AsNoTracking()
            .Include(e => e.Cliente)
            .OrderByDescending(e => e.DataCriacao);

        if (!string.IsNullOrEmpty(estado) && ConstantesEncomenda.TodosEstados.Contains(estado))
            query = query.Where(e => e.Estado == estado);

        var lista = await query.ToListAsync(cancellationToken);

        var totaisPorEstado = await _context.Encomendas
            .AsNoTracking()
            .GroupBy(e => e.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Estado, x => x.Total, cancellationToken);
        var totalGeral = totaisPorEstado.Values.Sum();

        ViewData["Estado"] = estado ?? "";
        ViewData["EstadosParaFiltro"] = ConstantesEncomenda.TodosEstados;
        ViewData["TotaisPorEstado"] = totaisPorEstado;
        ViewData["TotalGeral"] = totalGeral;
        return View(lista);
    }

    public async Task<IActionResult> Details(int? id, CancellationToken cancellationToken = default)
    {
        if (id == null) return NotFound();

        var encomenda = await _context.Encomendas
            .AsNoTracking()
            .Include(e => e.Cliente)
            .Include(e => e.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (encomenda == null) return NotFound();

        var stockPorProduto = await StockDisponivelService.ObterStockDisponivelPorProdutoAsync(_context, cancellationToken);
        ViewData["StockPorProduto"] = stockPorProduto;

        string? funcionarioAceiteNome = null;
        string? funcionarioPreparouNome = null;
        if (!string.IsNullOrEmpty(encomenda.FuncionarioAceiteUserId))
        {
            var uAceite = await _userManager.FindByIdAsync(encomenda.FuncionarioAceiteUserId);
            funcionarioAceiteNome = uAceite?.UserName ?? encomenda.FuncionarioAceiteUserId;
        }
        if (!string.IsNullOrEmpty(encomenda.FuncionarioPreparouUserId))
        {
            var uPreparou = await _userManager.FindByIdAsync(encomenda.FuncionarioPreparouUserId);
            funcionarioPreparouNome = uPreparou?.UserName ?? encomenda.FuncionarioPreparouUserId;
        }
        ViewData["FuncionarioAceiteNome"] = funcionarioAceiteNome;
        ViewData["FuncionarioPreparouNome"] = funcionarioPreparouNome;

        if (TempData["EncomendaCriada"] as bool? == true)
            TempData["MensagemSucesso"] = "Encomenda registada. Aguarde aceitação pela equipa.";
        if (TempData["EncomendaAceite"] as bool? == true)
            TempData["MensagemSucesso"] = "Encomenda aceite.";
        if (TempData["EncomendaRejeitada"] as bool? == true)
            TempData["MensagemSucesso"] = "Encomenda rejeitada e stock libertado.";
        if (TempData["EncomendaPreparacao"] as bool? == true)
            TempData["MensagemSucesso"] = "Preparação registada. Pode marcar como concluída quando os materiais forem entregues.";
        if (TempData["EncomendaConcluida"] as bool? == true)
            TempData["MensagemSucesso"] = "Encomenda concluída.";

        return View(encomenda);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(int? clienteId, CancellationToken cancellationToken = default)
    {
        var clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
        ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nome", clienteId);
        return View(new EncomendaCriarViewModel { ClienteId = clienteId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(EncomendaCriarViewModel model, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes.FindAsync(model.ClienteId);
        if (cliente == null)
            ModelState.AddModelError(nameof(model.ClienteId), "Selecione o cliente que fez a encomenda.");

        if (!ModelState.IsValid)
        {
            var clientes = await _context.Clientes.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
            ViewData["ClienteId"] = new SelectList(clientes, "Id", "Nome", model.ClienteId);
            return View(model);
        }

        return RedirectToAction(nameof(AdicionarItens), new { clienteId = model.ClienteId });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdicionarItens(
        int clienteId,
        string? pesquisa,
        string? classificacao,
        string? grupoCompatibilidade,
        string? filtroTecnico,
        string? calibre,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null) return NotFound();

        var draft = GetDraft();
        if (draft == null || draft.ClienteId != clienteId)
        {
            draft = new EncomendaDraftViewModel { ClienteId = clienteId, Itens = new List<EncomendaItemCriarViewModel>() };
            SetDraft(draft);
        }

        var query = _context.Produtos.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(pesquisa))
            query = query.Where(p => p.Nome.Contains(pesquisa));
        if (!string.IsNullOrEmpty(classificacao))
            query = query.Where(p => p.FamiliaRisco == classificacao);
        if (!string.IsNullOrEmpty(grupoCompatibilidade))
            query = query.Where(p => p.GrupoCompatibilidade == grupoCompatibilidade);
        if (!string.IsNullOrEmpty(filtroTecnico))
            query = query.Where(p => p.FiltroTecnico == filtroTecnico);
        if (!string.IsNullOrEmpty(calibre))
            query = query.Where(p => p.Calibre == calibre);

        var produtosFiltrados = await query.OrderBy(p => p.Nome).ToListAsync(cancellationToken);

        ViewData["ClienteNome"] = cliente.Nome;
        ViewData["ClienteId"] = clienteId;
        ViewData["Pesquisa"] = pesquisa ?? "";
        ViewData["Classificacao"] = classificacao ?? "";
        ViewData["GrupoCompatibilidade"] = grupoCompatibilidade ?? "";
        ViewData["FiltroTecnico"] = filtroTecnico ?? "";
        ViewData["Calibre"] = calibre ?? "";
        ViewData["ProdutosFiltrados"] = produtosFiltrados;
        ViewData["ItensRascunho"] = draft.Itens;

        return View(new EncomendaCriarViewModel { ClienteId = clienteId, Itens = draft.Itens });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdicionarItem(
        int clienteId,
        int produtoId,
        decimal quantidade,
        string? pesquisa,
        string? classificacao,
        string? grupoCompatibilidade,
        string? filtroTecnico,
        string? calibre,
        CancellationToken cancellationToken = default)
    {
        if (quantidade <= 0)
            return RedirectToAction(nameof(AdicionarItens), new { clienteId, pesquisa, classificacao, grupoCompatibilidade, filtroTecnico, calibre });

        var draft = GetDraft();
        if (draft == null || draft.ClienteId != clienteId)
            draft = new EncomendaDraftViewModel { ClienteId = clienteId, Itens = new List<EncomendaItemCriarViewModel>() };

        var produto = await _context.Produtos.FindAsync(produtoId);
        if (produto == null)
            return RedirectToAction(nameof(AdicionarItens), new { clienteId, pesquisa, classificacao, grupoCompatibilidade, filtroTecnico, calibre });

        var existente = draft.Itens.FirstOrDefault(i => i.ProdutoId == produtoId);
        if (existente != null)
            existente.Quantidade += quantidade;
        else
            draft.Itens.Add(new EncomendaItemCriarViewModel { ProdutoId = produtoId, ProdutoNome = produto.Nome, Quantidade = quantidade });

        SetDraft(draft);
        TempData["ItemAdicionado"] = $"{produto.Nome} ({quantidade}) adicionado à encomenda.";
        return RedirectToAction(nameof(AdicionarItens), new { clienteId, pesquisa, classificacao, grupoCompatibilidade, filtroTecnico, calibre });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public IActionResult RemoverItem(int clienteId, int produtoId, string? pesquisa, string? classificacao, string? grupoCompatibilidade, string? filtroTecnico, string? calibre)
    {
        var draft = GetDraft();
        if (draft != null && draft.ClienteId == clienteId)
        {
            draft.Itens.RemoveAll(i => i.ProdutoId == produtoId);
            SetDraft(draft);
        }
        return RedirectToAction(nameof(AdicionarItens), new { clienteId, pesquisa, classificacao, grupoCompatibilidade, filtroTecnico, calibre });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdicionarItens(int clienteId, CancellationToken cancellationToken = default)
    {
        var cliente = await _context.Clientes.FindAsync(clienteId);
        if (cliente == null)
        {
            ClearDraft();
            return RedirectToAction(nameof(Create));
        }

        var draft = GetDraft();
        if (draft == null || draft.ClienteId != clienteId || draft.Itens.Count == 0)
        {
            TempData["Erro"] = "Adicione pelo menos um produto à encomenda.";
            return RedirectToAction(nameof(AdicionarItens), new { clienteId });
        }

        var encomenda = new Encomenda
        {
            ClienteId = clienteId,
            Estado = ConstantesEncomenda.PENDENTE,
            DataCriacao = DateTime.UtcNow
        };
        _context.Encomendas.Add(encomenda);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var item in draft.Itens)
        {
            _context.EncomendaItems.Add(new EncomendaItem
            {
                EncomendaId = encomenda.Id,
                ProdutoId = item.ProdutoId,
                QuantidadePedida = item.Quantidade
            });
            _context.Reservas.Add(new Reserva
            {
                EncomendaId = encomenda.Id,
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade
            });
        }
        await _context.SaveChangesAsync(cancellationToken);

        var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _logSistema.RegistarAsync("ENCOMENDA_CRIADA", userId, User?.Identity?.Name, new { encomenda_id = encomenda.Id, cliente_id = encomenda.ClienteId }, cancellationToken);

        ClearDraft();
        TempData["EncomendaCriada"] = true;
        return RedirectToAction(nameof(Details), new { id = encomenda.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Aceitar(int id, CancellationToken cancellationToken = default)
    {
        var encomenda = await _context.Encomendas.FindAsync(id);
        if (encomenda == null) return NotFound();
        if (encomenda.Estado != ConstantesEncomenda.PENDENTE)
        {
            TempData["Erro"] = "Apenas encomendas pendentes podem ser aceites.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        encomenda.Estado = ConstantesEncomenda.ACEITE;
        encomenda.FuncionarioAceiteUserId = userId;
        await _context.SaveChangesAsync(cancellationToken);

        await _logSistema.RegistarAsync("ENCOMENDA_ACEITE", userId, User?.Identity?.Name, new { encomenda_id = id }, cancellationToken);

        TempData["EncomendaAceite"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Rejeitar(int? id, CancellationToken cancellationToken = default)
    {
        if (id == null) return NotFound();
        var encomenda = await _context.Encomendas.AsNoTracking().Include(e => e.Cliente).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (encomenda == null) return NotFound();
        if (encomenda.Estado != ConstantesEncomenda.PENDENTE && encomenda.Estado != ConstantesEncomenda.ACEITE)
        {
            TempData["Erro"] = "Apenas encomendas pendentes ou aceites podem ser rejeitadas.";
            return RedirectToAction(nameof(Details), new { id });
        }
        return View(encomenda);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Rejeitar(int id, string? motivoRejeicao, CancellationToken cancellationToken = default)
    {
        var encomenda = await _context.Encomendas.Include(e => e.Itens).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (encomenda == null) return NotFound();
        if (encomenda.Estado != ConstantesEncomenda.PENDENTE && encomenda.Estado != ConstantesEncomenda.ACEITE)
        {
            TempData["Erro"] = "Apenas encomendas pendentes ou aceites podem ser rejeitadas.";
            return RedirectToAction(nameof(Details), new { id });
        }

        encomenda.Estado = ConstantesEncomenda.REJEITADA;
        encomenda.MotivoRejeicao = string.IsNullOrWhiteSpace(motivoRejeicao) ? null : motivoRejeicao.Trim();
        var reservas = await _context.Reservas.Where(r => r.EncomendaId == id).ToListAsync(cancellationToken);
        _context.Reservas.RemoveRange(reservas);
        await _context.SaveChangesAsync(cancellationToken);

        var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _logSistema.RegistarAsync("ENCOMENDA_REJEITADA", userId, User?.Identity?.Name, new { encomenda_id = id, motivo = encomenda.MotivoRejeicao }, cancellationToken);

        TempData["EncomendaRejeitada"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Preparar(int? id, CancellationToken cancellationToken = default)
    {
        if (id == null) return NotFound();
        var encomenda = await _context.Encomendas
            .AsNoTracking()
            .Include(e => e.Cliente)
            .Include(e => e.Itens).ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (encomenda == null) return NotFound();
        if (encomenda.Estado != ConstantesEncomenda.ACEITE)
        {
            TempData["Erro"] = "Apenas encomendas aceites podem ser preparadas.";
            return RedirectToAction(nameof(Details), new { id });
        }
        var stockPorProduto = await StockDisponivelService.ObterStockDisponivelPorProdutoAsync(_context, cancellationToken);
        ViewData["StockPorProduto"] = stockPorProduto;
        return View(encomenda);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegistarPreparacao(int id, CancellationToken cancellationToken = default)
    {
        var encomenda = await _context.Encomendas.Include(e => e.Itens).ThenInclude(i => i.Produto).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (encomenda == null) return NotFound();
        if (encomenda.Estado != ConstantesEncomenda.ACEITE)
        {
            TempData["Erro"] = "Apenas encomendas aceites podem ser preparadas.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.GetUserAsync(User!);
        var rolesDoUtilizador = user == null ? Array.Empty<string>() : (await _userManager.GetRolesAsync(user)).ToArray();
        var idsPaióisComAcesso = await _context.PaiolAcessos
            .Where(a => rolesDoUtilizador.Contains(a.RoleName))
            .Select(a => a.PaiolId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var entradas = await _context.EntradasPaiol
            .Include(e => e.Paiol)
            .Include(e => e.Produto)
            .Where(e => idsPaióisComAcesso.Count == 0 || idsPaióisComAcesso.Contains(e.PaiolId))
            .ToListAsync(cancellationToken);
        var saidas = await _context.SaidasPaiol.Where(s => s.EntradaPaiolId != null).ToListAsync(cancellationToken);
        var restantePorEntrada = new Dictionary<int, decimal>();
        foreach (var e in entradas)
            restantePorEntrada[e.Id] = e.Quantidade;
        foreach (var s in saidas.Where(s => s.EntradaPaiolId.HasValue))
            restantePorEntrada[s.EntradaPaiolId!.Value] = restantePorEntrada.GetValueOrDefault(s.EntradaPaiolId.Value) - s.Quantidade;

        var entradasOrdenadasFifo = entradas
            .Where(e => restantePorEntrada.GetValueOrDefault(e.Id, 0) > 0)
            .OrderBy(e => e.DataFabrico ?? e.DataEntrada)
            .ThenBy(e => e.DataEntrada)
            .ToList();

        foreach (var item in encomenda.Itens)
        {
            var falta = item.QuantidadePedida;
            foreach (var ent in entradasOrdenadasFifo.Where(e => e.ProdutoId == item.ProdutoId))
            {
                if (falta <= 0) break;
                var rest = restantePorEntrada.GetValueOrDefault(ent.Id, 0);
                if (rest <= 0) continue;
                var qty = Math.Min(falta, rest);
                _context.SaidasPaiol.Add(new SaidaPaiol
                {
                    PaiolId = ent.PaiolId,
                    ProdutoId = ent.ProdutoId,
                    Quantidade = qty,
                    DataSaida = DateTime.UtcNow,
                    EncomendaId = encomenda.Id,
                    EntradaPaiolId = ent.Id,
                    FuncionarioRetirouUserId = userId
                });
                restantePorEntrada[ent.Id] = rest - qty;
                falta -= qty;

                await _logSistema.RegistarAsync("SAIDA_STOCK", userId, User?.Identity?.Name, new
                {
                    produto_id = ent.ProdutoId,
                    numero_lote = ent.NumeroLote,
                    quantidade_retirada_kg = qty,
                    paiol_id = ent.PaiolId,
                    paiol_nome = ent.Paiol?.Nome,
                    encomenda_id = encomenda.Id
                }, cancellationToken);
            }
            if (falta > 0)
            {
                TempData["Erro"] = $"Stock insuficiente para preparar o item {item.Produto?.Nome}. Falta alocar {falta:N2}. Se não houver stock disponível, pode rejeitar a encomenda em Detalhes para libertar a reserva.";
                return RedirectToAction(nameof(Preparar), new { id });
            }
        }

        encomenda.Estado = ConstantesEncomenda.EM_PREPARACAO;
        encomenda.FuncionarioPreparouUserId = userId;
        await _context.SaveChangesAsync(cancellationToken);

        TempData["EncomendaPreparacao"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Concluir(int id, CancellationToken cancellationToken = default)
    {
        var encomenda = await _context.Encomendas.FindAsync(id);
        if (encomenda == null) return NotFound();
        if (encomenda.Estado != ConstantesEncomenda.EM_PREPARACAO)
        {
            TempData["Erro"] = "Apenas encomendas em preparação podem ser concluídas.";
            return RedirectToAction(nameof(Details), new { id });
        }

        encomenda.Estado = ConstantesEncomenda.CONCLUIDA;
        encomenda.DataConclusao = DateTime.UtcNow;
        var reservas = await _context.Reservas.Where(r => r.EncomendaId == id).ToListAsync(cancellationToken);
        _context.Reservas.RemoveRange(reservas);
        await _context.SaveChangesAsync(cancellationToken);

        var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        await _logSistema.RegistarAsync("ENCOMENDA_CONCLUIDA", userId, User?.Identity?.Name, new { encomenda_id = id }, cancellationToken);

        TempData["EncomendaConcluida"] = true;
        return RedirectToAction(nameof(Details), new { id });
    }
}
