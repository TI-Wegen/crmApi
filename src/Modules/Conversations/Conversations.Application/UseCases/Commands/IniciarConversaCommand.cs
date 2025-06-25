using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record IniciarConversaCommand(
    Guid ContatoId,
    string TextoDaPrimeiraMensagem,
     Stream? AnexoStream,
    string? AnexoNome,
    string? AnexoContentType
) : ICommand;