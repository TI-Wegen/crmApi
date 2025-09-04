using CRM.Application.Interfaces;

namespace Tags.Application.UseCases.Commands;

public record CriarTagCommand(
    string Nome,
    string Cor,
    string? Descricao
) : ICommand;