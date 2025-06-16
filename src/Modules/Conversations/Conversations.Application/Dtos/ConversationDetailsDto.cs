namespace Conversations.Application.Dtos;

public record ConversationDetailsDto
{
    public Guid Id { get; init; }
    public Guid ContatoId { get; init; }
    public Guid? AgenteId { get; init; }
    public Guid? SetorId { get; init; }
    public string Status { get; init; } // Enviamos como string para ser facilmente consumível
    public IReadOnlyCollection<MessageDto> Mensagens { get; init; } = new List<MessageDto>();
}

// DTO aninhado para as mensagens
public record MessageDto
{
    public Guid Id { get; init; }
    public string Texto { get; init; }
    public string? AnexoUrl { get; init; }
    public DateTime Timestamp { get; init; }
    public string RemetenteTipo { get; init; } // "Agente" ou "Cliente"
    public Guid? RemetenteAgenteId { get; init; }
}