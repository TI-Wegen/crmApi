using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries.Handlers;

public class GetConversationByIdQueryHandler : IQueryHandler<GetConversationByIdQuery, ConversationDetailsDto>
{
    private readonly IConversationRepository _conversationRepository;

    public GetConversationByIdQueryHandler(IConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
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

        // 3. Mapeamos o agregado de domínio para o DTO de resposta.
        // Nada de UnitOfWork.SaveChangesAsync() aqui, pois é uma operação de leitura.

        return conversa.ToDetailsDto();
    }
}