using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record ResolverAtendimentoPorInatividadeCommand(Guid AtendimentoId) : ICommand;

