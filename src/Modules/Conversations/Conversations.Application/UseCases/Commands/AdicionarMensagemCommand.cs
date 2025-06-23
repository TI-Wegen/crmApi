using Conversations.Domain.Enuns;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AdicionarMensagemCommand(
    Guid ConversaId,
    string Texto,
    string? AnexoUrl,
    RemetenteTipo RemetenteTipo, // Precisamos saber QUEM está enviando
    Guid? AgenteId = null         // Necessário se o remetente for um Agente
) : ICommand;