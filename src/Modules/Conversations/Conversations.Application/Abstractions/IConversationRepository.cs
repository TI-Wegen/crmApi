using Conversations.Domain.Aggregates;
using System;

namespace Conversations.Application.Abstractions;

public interface IConversationRepository
{
    Task<Conversa?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Conversa conversa, CancellationToken cancellationToken = default);
    Task UpdateAsync(Conversa conversa, CancellationToken cancellationToken = default);
    Task<Conversa?> GetByIdWithMessagesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Conversa>> GetConversasAtivasCriadasAntesDeAsync(DateTime limite, CancellationToken cancellationToken = default);

}
