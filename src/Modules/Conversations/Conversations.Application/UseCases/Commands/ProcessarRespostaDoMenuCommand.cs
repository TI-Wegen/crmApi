using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;
public record ProcessarRespostaDoMenuCommand(string ContatoTelefone, string TextoDaResposta) : ICommand;
