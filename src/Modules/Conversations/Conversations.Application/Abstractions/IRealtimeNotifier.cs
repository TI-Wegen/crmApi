using Conversations.Application.Dtos;

namespace Conversations.Application.Abstractions;

public interface IRealtimeNotifier
{
    Task NotificarNovaMensagemAsync(string conversationId, MessageDto messageDto);
    Task NotificarNovaConversaNaFilaAsync(ConversationSummaryDto conversationDto);
}