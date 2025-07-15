using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;
public record ResolverAtendimentoCommand(Guid AtendimentoId) : ICommand;

