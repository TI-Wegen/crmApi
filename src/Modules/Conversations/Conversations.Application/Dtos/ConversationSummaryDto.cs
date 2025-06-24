namespace Conversations.Application.Dtos;

public record ConversationSummaryDto
{
    public Guid Id { get; init; }
    public string ContatoNome { get; init; }
    public string ContatoTelefone { get; init; }
    public string? AgenteNome { get; init; }
    public string Status { get; init; }
    public DateTime UltimaMensagemTimestamp { get; init; }
    public string UltimaMensagemPreview { get; init; }
}