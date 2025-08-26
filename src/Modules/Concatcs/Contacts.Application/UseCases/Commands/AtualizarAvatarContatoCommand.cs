using CRM.Application.Interfaces;

namespace Contacts.Application.UseCases.Commands;

public record AtualizarAvatarContatoCommand(string WaId) : ICommand;