using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record ReabrirConversaCommand(Guid ConversaId) : ICommand;
