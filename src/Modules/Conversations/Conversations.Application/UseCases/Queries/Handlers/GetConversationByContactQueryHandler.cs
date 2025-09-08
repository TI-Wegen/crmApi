using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries.Handlers;

public class GetConversationByContactQueryHandler : IQueryHandler<GetConversationByContactQuery, ConversationDetailsDto>
{
    private readonly IConversationReadService _readService;

    public GetConversationByContactQueryHandler(
        IConversationReadService readService)
    {
        _readService = readService;
    }

    public async Task<ConversationDetailsDto> HandleAsync(GetConversationByContactQuery query,
        CancellationToken cancellationToken = default)
    {
        var conversationDetails = await _readService.GetConversationDetailsByContactAsync(
            query.ContactId, 
            query.PageNumber,
            query.PageSize, 
            cancellationToken);

        if (conversationDetails is null)
        {
            throw new NotFoundException($"Conversa com o Id do contato '{query.ContactId}' não encontrada.");
        }

        return conversationDetails;
    }
}