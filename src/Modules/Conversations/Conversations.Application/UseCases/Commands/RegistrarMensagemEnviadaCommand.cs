using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record RegistrarMensagemEnviadaCommand(
    string ContatoTelefone,
    string NomeContato,
    string TextoDaMensagem,
    string? IdDaMensagemMeta,
    Guid? IdConversa = null
) : ICommand;