namespace CRM.API.Services;


using Conversations.Application.Abstractions;
// Em Api/Services/ (pode criar esta pasta)
using Conversations.Application.Dtos;
using CRM.API.Hubs;
using Microsoft.AspNetCore.SignalR;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<ConversationHub> _hubContext;
    private const string UnassignedQueueGroupName = "UnassignedQueue"; 
    public SignalRNotifier(IHubContext<ConversationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarNovaMensagemAsync(string conversationId, MessageDto messageDto)
    {
        // Envia uma mensagem para um grupo específico.
        // O primeiro argumento "ReceiveMessage" é o nome do método que o JAVASCRIPT
        // no frontend estará ouvindo.
        await _hubContext.Clients
            .Group(conversationId)
            .SendAsync("ReceiveMessage", messageDto);
    }

    public async Task NotificarNovaConversaNaFilaAsync(ConversationSummaryDto conversationDto)
    {
        // Envia uma mensagem para o grupo geral dos agentes.
        // O evento se chamará "ReceiveNewConversation".
        await _hubContext.Clients
            .Group(UnassignedQueueGroupName)
            .SendAsync("ReceiveNewConversation", conversationDto);
    }
}