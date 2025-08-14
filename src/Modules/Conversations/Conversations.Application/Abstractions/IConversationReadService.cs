using Conversations.Application.Dtos;
using Conversations.Application.UseCases.Queries;
using CRM.Application.Mappers;

namespace Conversations.Application.Abstractions;

public interface IConversationReadService
{
    Task<IEnumerable<ConversationSummaryDto>> GetAllSummariesAsync(
        GetAllConversationsQuery query,
        CancellationToken cancellationToken = default);

    Task<ConversationSummaryDto?> GetSummaryByIdAsync(Guid conversationId, CancellationToken cancellationToken = default);
    Task<ConversationDetailsDto?> GetConversationDetailsAsync(Guid conversationId, CancellationToken cancellationToken = default);


}
