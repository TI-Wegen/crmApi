namespace CRM.API.Hubs;


// Em Api/Hubs/
using Microsoft.AspNetCore.SignalR;

public class ConversationHub : Hub
{
    private const string UnassignedQueueGroupName = "UnassignedQueue";
    private readonly ILogger<ConversationHub>? _logger;

    public ConversationHub(ILogger<ConversationHub>? logger)
    {
        _logger = logger;
    }
    public override async Task OnConnectedAsync()
    {
        _logger?.LogInformation("Usuário conectado: ConnectionId = {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger?.LogWarning("Usuário desconectado: ConnectionId = {ConnectionId}, Erro: {Error}",
            Context.ConnectionId, exception?.Message);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversationGroup(string conversationId)
    {
        if (string.IsNullOrWhiteSpace(conversationId))
        {
            _logger?.LogWarning("Tentativa de entrada com conversationId vazio. ConnectionId: {ConnectionId}", Context.ConnectionId);
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        _logger?.LogInformation("Connection {ConnectionId} entrou no grupo: {ConversationId}", Context.ConnectionId, conversationId);
    }


    public async Task LeaveConversationGroup(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        _logger.LogInformation("Connection {ConnectionId} saiu do grupo da conversa: {ConversationId}", Context.ConnectionId, conversationId);

    }
    public async Task JoinUnassignedQueue()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, UnassignedQueueGroupName);
    }
}