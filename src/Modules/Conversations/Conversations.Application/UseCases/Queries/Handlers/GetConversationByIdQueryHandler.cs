using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries.Handlers;

public class GetConversationByIdQueryHandler : IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository;


    public GetConversationByIdQueryHandler(IConversationRepository conversationRepository, 
        IAtendimentoRepository atendimentoRepository)
    {
        _conversationRepository = conversationRepository;
        _atendimentoRepository = atendimentoRepository;
    }

    public async Task<ConversationDetailsDto> HandleAsync(GetConversationByIdQuery query, CancellationToken cancellationToken)
    {
        // 1. Buscamos o agregado, incluindo suas mensagens.
        // Precisaremos adicionar este novo método ao nosso repositório.
        var conversa = await _conversationRepository.GetByIdWithMessagesAsync(query.ConversaId, cancellationToken);

        // 2. Validamos a existência.
        if (conversa is null)
        {
            throw new NotFoundException($"Conversa com o Id '{query.ConversaId}' não encontrada.");
        }

        var atendimentoAtivo = await _atendimentoRepository.FindActiveByConversaIdAsync(query.ConversaId, cancellationToken);

        return conversa.ToDetailsDto(atendimentoAtivo);
    }
}