using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AtribuirAgenteCommand(
    Guid AtendimentoId,
    Guid AgenteId
) : ICommand;
