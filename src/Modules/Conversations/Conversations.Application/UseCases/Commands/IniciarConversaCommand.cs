using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record IniciarConversaCommand(
    Guid ContatoId,
    string TextoDaPrimeiraMensagem,
    string? AnexoUrl 
) : ICommand;