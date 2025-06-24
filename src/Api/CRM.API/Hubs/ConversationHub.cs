namespace CRM.API.Hubs;


// Em Api/Hubs/
using Microsoft.AspNetCore.SignalR;

public class ConversationHub : Hub
{
    /// <summary>
    /// Método que o cliente chamará ao entrar na tela de uma conversa específica.
    /// Ele adiciona a conexão do cliente a um grupo nomeado com o ID da conversa.
    /// </summary>
    public async Task JoinConversationGroup(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    /// <summary>
    /// Método que o cliente chamará ao sair da tela da conversa.
    /// </summary>
    public async Task LeaveConversationGroup(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }
}