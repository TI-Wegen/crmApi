using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AdicionaReacaoMensagemCommand(
    string Emoji,
    string MessageId
    ) : ICommand;