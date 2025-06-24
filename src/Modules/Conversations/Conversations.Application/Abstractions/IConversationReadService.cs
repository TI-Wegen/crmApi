using Conversations.Application.Dtos;
using Conversations.Application.UseCases.Queries;

namespace Conversations.Application.Abstractions;

public interface IConversationReadService
{
    Task<IEnumerable<ConversationSummaryDto>> GetAllSummariesAsync(
        GetAllConversationsQuery query,
        CancellationToken cancellationToken = default);

    Task<ConversationSummaryDto?> GetSummaryByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);

}
