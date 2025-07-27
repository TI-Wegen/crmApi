using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries.Handlers;

public class GetConversationByIdQueryHandler : IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto>
{

    private readonly IConversationReadService _readService;


    public GetConversationByIdQueryHandler(
        IConversationReadService readService)
    {

        _readService = readService;
    }

    public async Task<ConversationDetailsDto> HandleAsync(GetConversationByIdQuery query, CancellationToken cancellationToken)
    {
        var conversationDetails = await _readService.GetConversationDetailsAsync(query.ConversaId, cancellationToken);

        if (conversationDetails is null)
        {
            throw new NotFoundException($"Conversa com o Id '{query.ConversaId}' não encontrada.");
        }

        return conversationDetails;
    }
}