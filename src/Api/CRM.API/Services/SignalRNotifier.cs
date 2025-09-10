using Conversations.Application.Abstractions;
using Conversations.Application.Dtos;
using CRM.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CRM.API.Services;

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
            Timestamp = DateTime.Now,
            RemetenteTipo = messageDto.RemetenteTipo,
            RemetenteAgenteId = messageDto.RemetenteAgenteId
        };

        await _hubContext.Clients
            .Group(conversationId)
            .SendAsync("ReceiveMessage", enrichedMessage);

        await _hubContext.Clients
            .Group("UnassignedQueue")
            .SendAsync("ReceiveMessage", enrichedMessage);
    }

    public async Task NotificarNovaConversaNaFilaAsync(ConversationSummaryDto conversationDto)
    {
        await _hubContext.Clients
            .Group(UnassignedQueueGroupName)
            .SendAsync("ReceiveNewConversation", conversationDto);
    }
}