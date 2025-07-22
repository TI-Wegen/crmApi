using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record IniciarConversaCommand(
    Guid ContatoId,
    string TextoDaMensagem,
       DateTime? Timestamp,
       string? MessageId = null,
       string? AnexoUrl = null,
     Stream? AnexoStream = null,
    string? AnexoNome = null,
    string? AnexoContentType = null,
    bool IniciarComBot = true
) : ICommand;