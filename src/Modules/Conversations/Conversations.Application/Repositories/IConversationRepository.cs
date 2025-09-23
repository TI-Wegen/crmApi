using Conversations.Domain.Entities;

namespace Conversations.Application.Repositories;

public interface IConversationRepository
{
    Task<Conversa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Conversa conversa, CancellationToken cancellationToken = default);
    Task UpdateAsync(Conversa conversa, CancellationToken cancellationToken = default);
    Task<Conversa?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Conversa?> FindActiveByContactIdAsync(Guid contactId, CancellationToken cancellationToken = default);
    void MarkAsUnchanged(Conversa conversa);
    Task AddTagAtendimento(Guid contactId, Guid tagId, CancellationToken cancellationToken);
}