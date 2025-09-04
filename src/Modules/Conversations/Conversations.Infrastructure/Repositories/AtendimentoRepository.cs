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
    public async Task<Atendimento?> FindActiveByConversaIdAsync(Guid conversaId,
        CancellationToken cancellationToken = default)
    {
        var inactiveStatuses = new[] { ConversationStatus.Resolvida };

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
    public async Task<IEnumerable<Atendimento>> GetAtendimentosAtivosCriadosAntesDeAsync(DateTime dataLimite,
        CancellationToken cancellationToken = default)
    {
        var activeStatuses = new[]
        {
            ConversationStatus.EmAutoAtendimento,
            ConversationStatus.AguardandoNaFila,
            ConversationStatus.EmAtendimento
        };

        return await _context.Atendimentos
            .Where(a => activeStatuses.Contains(a.Status) && a.CreatedAt < dataLimite)
            .ToListAsync(cancellationToken);
    }
    public async Task<IEnumerable<Atendimento>> GetLastTwoByConversaIdAsync(Guid conversaId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Atendimentos
            .Where(a => a.ConversaId == conversaId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(2);

        return await query.ToListAsync(cancellationToken);
    }
    public async Task<IEnumerable<Atendimento>> GetAtendimentosEmAutoAtendimentoAsync(
        CancellationToken cancellationToken = default)
    {
        var botStatuses = new[]
        {
            ConversationStatus.EmAutoAtendimento
        };

        return await _context.Atendimentos
            .Where(a => botStatuses.Contains(a.Status))
            .ToListAsync(cancellationToken);
    }
    public Task AddTagAtendimento(Guid contactId, Guid tagId, CancellationToken cancellationToken)
    {
        var atendimento = _context.Atendimentos.FirstOrDefault(x => x.ConversaId == contactId);

        if (atendimento is null)
        {
            throw new Exception("Atendimmento não enocntrado");
        }
        
        atendimento.TagsId = tagId;
        
        return _context.SaveChangesAsync(cancellationToken);
    }
}