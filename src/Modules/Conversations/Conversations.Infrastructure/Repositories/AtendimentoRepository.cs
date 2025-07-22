using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Enuns;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;


namespace Conversations.Infrastructure.Repositories;

public class AtendimentoRepository : IAtendimentoRepository
{
    private readonly AppDbContext _context;

    public AtendimentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Atendimento atendimento, CancellationToken cancellationToken = default)
    {
        await _context.Atendimentos.AddAsync(atendimento, cancellationToken);
    }

    public async Task<Atendimento?> FindActiveByConversaIdAsync(Guid conversaId, CancellationToken cancellationToken = default)
    {
        var inactiveStatuses = new[] { ConversationStatus.Resolvida, ConversationStatus.SessaoExpirada };

        return await _context.Atendimentos
            .FirstOrDefaultAsync(a =>
                a.ConversaId == conversaId &&
                !inactiveStatuses.Contains(a.Status),
                cancellationToken);
    }

    public async Task<Atendimento?> GetByIdAsync(Guid atendimentoId, CancellationToken cancellationToken = default)
    {
        return await _context.Atendimentos.FindAsync(atendimentoId, cancellationToken);
    }

    public async Task<IEnumerable<Atendimento>> GetAtendimentosAtivosCriadosAntesDeAsync(DateTime dataLimite, CancellationToken cancellationToken = default)
    {
        // Define quais status são considerados ativos e podem expirar.
        var activeStatuses = new[]
        {
        ConversationStatus.EmAutoAtendimento,
        ConversationStatus.AguardandoNaFila,
        ConversationStatus.EmAtendimento
    };

        // Busca todos os atendimentos que estão em um status ativo E foram criados antes da data limite.
        return await _context.Atendimentos
            .Where(a => activeStatuses.Contains(a.Status) && a.CreatedAt < dataLimite) // Supondo que você tenha uma propriedade CreatedAt
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Atendimento>> GetLastTwoByConversaIdAsync(Guid conversaId, CancellationToken cancellationToken = default)
    {
        var query = _context.Atendimentos
            .Where(a => a.ConversaId == conversaId)
            .OrderByDescending(a => a.CreatedAt) // Ordena pela data de criação mais recente
            .Take(2); // Limita a dois resultados

        return await query.ToListAsync(cancellationToken);
    }
    public async Task<IEnumerable<Atendimento>> GetAtendimentosEmAutoAtendimentoAsync(CancellationToken cancellationToken = default)
    {
        // Define quais status são considerados de autoatendimento
        var botStatuses = new[]
        {
        ConversationStatus.EmAutoAtendimento
    };

        return await _context.Atendimentos
            .Where(a => botStatuses.Contains(a.Status))
            .ToListAsync(cancellationToken);
    }
}