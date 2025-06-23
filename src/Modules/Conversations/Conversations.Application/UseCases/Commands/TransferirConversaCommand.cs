using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record TransferirConversaCommand(
    Guid ConversaId,
    Guid NovoAgenteId,
    Guid NovoSetorId
) : ICommand;
