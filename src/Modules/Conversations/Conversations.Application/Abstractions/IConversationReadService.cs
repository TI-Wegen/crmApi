using Conversations.Application.Dtos;
using Conversations.Application.UseCases.Queries;

namespace Conversations.Application.Abstractions;

public interface IConversationReadService
{
    Task<IEnumerable<ConversationSummaryDto>> GetAllSummariesAsync(
        GetAllConversationsQuery query,
        CancellationToken cancellationToken = default);

    Task<ConversationSummaryDto?> GetSummaryByIdAsync(Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<ConversationDetailsDto?> GetConversationDetailsAsync(Guid conversationId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    Task<ConversationDetailsDto?> GetConversationDetailsByContactAsync(Guid contactId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}