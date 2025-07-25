namespace Conversations.Application.Dtos;

public record ConversationDetailsDto
{
    // Informações da Conversa (Histórico)
    public Guid Id { get; init; }
    public Guid ContatoId { get; init; }
    public IReadOnlyCollection<MessageDto> Mensagens { get; init; } = new List<MessageDto>();

    // Informações do Atendimento Atual
    public Guid? AtendimentoId { get; init; }
    public Guid? AgenteId { get; init; }
    public Guid? SetorId { get; init; }
    public string Status { get; init; }
    public string BotStatus { get; init; }
    public bool SessaoWhatsappAtiva { get; init; }
    public DateTime? SessaoWhatsappExpiraEm { get; init; }
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


public record MessageWithConversationIdDto : MessageDto
{
    public string ConversationId { get; set; } = default!;
}
