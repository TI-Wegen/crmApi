namespace Conversations.Infrastructure.Repositories;

using Conversations.Application.Abstractions;
using Conversations.Domain.Aggregates;
using Conversations.Domain.Enuns;
using CRM.Infrastructure.Database;
// Em Infrastructure/Repositories/
using Microsoft.EntityFrameworkCore;
using System;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _context;
    public ConversationRepository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }


    public async Task AddAsync(Conversa conversa, CancellationToken cancellationToken = default)
    {
        await _context.Conversas.AddAsync(conversa, cancellationToken);
    }
    public Task<Conversa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Conversas.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<Conversa?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default)
    {
   
        return _context.Conversas
            .Include(c => c.Mensagens)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
    public  Task UpdateAsync(Conversa conversa, CancellationToken cancellationToken = default)
    {
        _context.Conversas.Update(conversa);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<Conversa>> GetConversasAtivasCriadasAntesDeAsync(DateTime limite, CancellationToken cancellationToken = default)
    {
        return await _context.Conversas
         .Where(c => c.Status == ConversationStatus.AguardandoNaFila || c.Status == ConversationStatus.EmAtendimento)
         .ToListAsync(cancellationToken);
    }

    public Task<Conversa?> FindActiveByContactIdAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        return _context.Conversas
            .Include(c => c.Mensagens) // Manter o Include é uma boa prática.
            .FirstOrDefaultAsync(c =>
                c.ContatoId == contactId,
                cancellationToken);
    }

    public void MarkAsUnchanged(Conversa conversa)
    {
        _context.Entry(conversa).State = EntityState.Unchanged;
    }
}