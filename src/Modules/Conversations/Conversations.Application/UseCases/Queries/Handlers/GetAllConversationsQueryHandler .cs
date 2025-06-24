
using CRM.Application.Interfaces;


namespace Conversations.Application.UseCases.Queries.Handlers;

using Conversations.Application.Abstractions;


using Conversations.Application.Dtos;


public class GetAllConversationsQueryHandler : IQueryHandler<GetAllConversationsQuery, IEnumerable<ConversationSummaryDto>>
{
    // A dependência agora é na ABSTRAÇÃO, não mais em IDbConnection
    private readonly IConversationReadService _readService;

    public GetAllConversationsQueryHandler(IConversationReadService readService)
    {
        _readService = readService;
    }

    public async Task<IEnumerable<ConversationSummaryDto>> HandleAsync(GetAllConversationsQuery query, CancellationToken cancellationToken)
    {
        // O handler apenas delega a chamada. Simples, limpo e testável.
        return await _readService.GetAllSummariesAsync(query, cancellationToken);
    }
}