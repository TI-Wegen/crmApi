using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AtribuirAgenteCommand(
    Guid ConversaId,
    Guid AgenteId
) : ICommand;
