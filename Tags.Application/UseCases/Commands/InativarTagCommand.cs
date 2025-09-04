using CRM.Application.Interfaces;

namespace Tags.Application.UseCases.Commands;

public record InativarTagCommand(Guid Guid) : ICommand;