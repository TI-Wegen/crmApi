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
        var enrichedMessage = new MessageWithConversationIdDto
        {
            ConversationId = conversationId,
            Id = messageDto.Id,
            Texto = messageDto.Texto,
            AnexoUrl = messageDto.AnexoUrl,
            Timestamp = messageDto.Timestamp,
            RemetenteTipo = messageDto.RemetenteTipo,
            RemetenteAgenteId = messageDto.RemetenteAgenteId
        };

        await _hubContext.Clients
            .Group(conversationId)
            .SendAsync("ReceiveMessage", enrichedMessage);
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