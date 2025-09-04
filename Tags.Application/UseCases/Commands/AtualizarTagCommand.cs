using CRM.Application.Interfaces;

namespace Tags.Application.UseCases.Commands;

public record AtualizarTagCommand(
    Guid Id,
    string Nome,
    string Cor,
    string? Descricao
    ) : ICommand;