namespace CRM.API.Hubs;


// Em Api/Hubs/
using Microsoft.AspNetCore.SignalR;

public class ConversationHub : Hub
{
    private const string UnassignedQueueGroupName = "UnassignedQueue";

    public async Task JoinConversationGroup(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
    }

    public async Task LeaveConversationGroup(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
    }
    public async Task JoinUnassignedQueue()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, UnassignedQueueGroupName);
    }
}