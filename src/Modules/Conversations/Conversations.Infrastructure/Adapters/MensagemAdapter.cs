using Conversations.Application.Repositories;
using Conversations.Domain.Entities;
using CRM.Infrastructure.Database;

namespace Conversations.Infrastructure.Adapters;

public class MensagemAdapter : IMensagemRepository
{
    private readonly AppDbContext _context;

    public MensagemAdapter(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public Task UpdateAsync(Mensagem mensagem, CancellationToken cancellationToken = default)
    {
        _context.Mensagens.Update(mensagem);
        return Task.CompletedTask;
    }
    
    public async Task<Mensagem?> FindMessageByExternalIdAsync(string externalId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(externalId))
            return null;

        return _context.Mensagens.FirstOrDefault(m => m.ExternalId == externalId);
    }
}