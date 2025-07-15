using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using Conversations.Application.Mappers;
using CRM.Application.Exceptions;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Queries.Handlers;

public class GetActiveChatQueryHandler : IQueryHandler<GetActiveChatQuery, ActiveChatDto>
{
    private  readonly IConversationRepository  _conversationRepository;
    private readonly IAtendimentoRepository _atendimentoRepository;

    public GetActiveChatQueryHandler(IConversationRepository conversationRepository, 
        IAtendimentoRepository atendimentoRepository)
    {
        _conversationRepository = conversationRepository;
        _atendimentoRepository = atendimentoRepository;
    }

    public async Task<ActiveChatDto> HandleAsync(GetActiveChatQuery query, CancellationToken cancellationToken = default)
    {
        // 1. Busca a conversa com TODAS as mensagens
        var conversa = await _conversationRepository.GetByIdWithMessagesAsync(query.ConversaId);
        if (conversa is null) throw new NotFoundException("Conversa não encontrada.");

        // 2. Busca os DOIS últimos atendimentos para esta conversa
        var ultimosAtendimentos = await _atendimentoRepository.GetLastTwoByConversaIdAsync(query.ConversaId);

        var atendimentoAtual = ultimosAtendimentos.FirstOrDefault();
        var atendimentoAnterior = ultimosAtendimentos.Skip(1).FirstOrDefault();

        if (atendimentoAtual is null) throw new NotFoundException("Nenhum atendimento encontrado.");

        // 3. Filtra as mensagens para pertencer apenas a estes dois atendimentos
        var idsDosAtendimentos = ultimosAtendimentos.Select(a => a.Id).ToList();
        var mensagensRelevantes = conversa.Mensagens
            .Where(m => idsDosAtendimentos.Contains(m.AtendimentoId))
            .Select(m => m.ToDto())
            .ToList();

        // 4. Monta e retorna o DTO final
        return new ActiveChatDto
        {
            AtendimentoAtual = atendimentoAtual.ToDto(),
            AtendimentoAnterior = atendimentoAnterior?.ToDto(),
            Mensagens = mensagensRelevantes
        };
    }
}


