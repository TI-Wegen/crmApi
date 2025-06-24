namespace CRM.API.Services;


using Api.Hubs; // Onde o ConversationHub está
using Conversations.Application.Abstractions;
// Em Api/Services/ (pode criar esta pasta)
using Conversations.Application.Dtos;
using Conversations.Application.Ports;
using CRM.API.Hubs;
using Microsoft.AspNetCore.SignalR;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<ConversationHub> _hubContext;

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
}