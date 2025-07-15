using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record TransferirAtendimentoCommand(
    Guid AtendimentoId,
    Guid NovoAgenteId,
    Guid NovoSetorId
) : ICommand;
