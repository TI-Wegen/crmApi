using Conversations.Domain.Enuns;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AdicionarMensagemCommand(
    Guid ConversaId,
    string Texto,
    RemetenteTipo RemetenteTipo,
    Stream? AnexoStream,
    string? AnexoNome,
    string? AnexoContentType,
    Guid? AgenteId = null
) : ICommand;