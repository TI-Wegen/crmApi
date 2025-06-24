using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Commands;

public record InativarAgenteCommand(Guid AgenteId) : ICommand;
