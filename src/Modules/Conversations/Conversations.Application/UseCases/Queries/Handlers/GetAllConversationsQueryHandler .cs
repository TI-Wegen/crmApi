using Conversations.Application.Services;
using CRM.Application.Interfaces;


namespace Conversations.Application.UseCases.Queries.Handlers;

using Conversations.Application.Dtos;


public class GetAllConversationsQueryHandler : IQueryHandler<GetAllConversationsQuery, IEnumerable<ConversationSummaryDto>>
{
    private readonly IConversationReadService _readService;

    public GetAllConversationsQueryHandler(IConversationReadService readService)
    {
        _readService = readService;
    }

    public async Task<IEnumerable<ConversationSummaryDto>> HandleAsync(GetAllConversationsQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetAllSummariesAsync(query, cancellationToken);
    }
}