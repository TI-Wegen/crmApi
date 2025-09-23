using Conversations.Application.Repositories;
using Conversations.Domain.Entities;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Conversations.Infrastructure.Adapters;

public class ConversationAdapter : IConversationRepository
{
    private readonly AppDbContext _context;

    public ConversationAdapter(AppDbContext context)
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

    public Task UpdateAsync(Conversa conversa, CancellationToken cancellationToken = default)
    {
        _context.Conversas.Update(conversa);
        return Task.CompletedTask;
    }

    public Task<Conversa?> FindActiveByContactIdAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        return _context.Conversas
            .Include(c => c.Mensagens)
            .FirstOrDefaultAsync(c =>
                    c.ContatoId == contactId,
                cancellationToken);
    }

    public void MarkAsUnchanged(Conversa conversa)
    {
        _context.Entry(conversa).State = EntityState.Unchanged;
    }

    public async Task AddTagAtendimento(Guid contactId, Guid tagId, CancellationToken cancellationToken)
    {
        var atendimento = await _context.Conversas
            .FirstOrDefaultAsync(x => x.ContatoId == contactId, cancellationToken);

        if (atendimento is null)
        {
            throw new Exception("Conversa não encontrada");
        }

        atendimento.TagsId = tagId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}