using CRM.Application.Interfaces;

namespace Agents.Application.UseCases.Commands;
public record CriarAgenteCommand(
    string Nome,
    string Email,
    string Senha
) : ICommand;