
namespace CRM.Application.Interfaces;
using CRM.Application.Mappers;

public interface IRealtimeNotifier
{
    Task NotificarNovaMensagemAsync(string conversationId, MessageDto messageDto);
    Task NotificarNovaConversaNaFilaAsync(ConversationSummaryDto conversationDto);

}