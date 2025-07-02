using Conversations.Domain.Enuns;
using CRM.Application.Interfaces;

namespace Conversations.Application.UseCases.Commands;

public record AdicionarMensagemCommand(
    Guid ConversaId,
    string Texto,
    string? AnexoUrl,
    RemetenteTipo RemetenteTipo,
    Stream? AnexoStream,
    string? AnexoNome,
    string? AnexoContentType
) : ICommand;