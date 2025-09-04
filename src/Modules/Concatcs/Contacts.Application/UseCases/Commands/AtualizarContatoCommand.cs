using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands;

public record AtualizarContatoCommand(
    Guid ContactId,
    string NovoNome,
    string NovoTelefone,
    Guid? tag
) : ICommand;