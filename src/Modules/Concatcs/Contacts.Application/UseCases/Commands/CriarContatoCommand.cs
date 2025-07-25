using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands;

public record CriarContatoCommand(
    string Nome,
    string Telefone,
    string WaId 
) : ICommand;