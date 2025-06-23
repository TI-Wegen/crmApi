using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;
public record ResolverConversaCommand(Guid ConversaId) : ICommand;

