using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands;
public record InativarContatoCommand(Guid ContactId) : ICommand;


