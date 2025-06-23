using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Commands;

public record AtualizarAgenteCommand(
    Guid AgenteId,
    string NovoNome,
    List<Guid> NovosSetorIds
) : ICommand;