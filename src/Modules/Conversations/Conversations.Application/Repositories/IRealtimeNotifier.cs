using Conversations.Application.Dtos;

namespace Conversations.Application.Repositories;

public interface IRealtimeNotifier
{
    Task NotificarNovaMensagemAsync(string conversationId, MessageDto messageDto);
    Task NotificarNovaConversaNaFilaAsync(ConversationSummaryDto conversationDto);
}