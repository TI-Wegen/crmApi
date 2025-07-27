namespace Conversations.Application.Dtos;

public class ConversationDetailsDto
{
    // Informações da Conversa (Histórico)
    public Guid Id { get; set; }
    public Guid ContatoId { get; set; }
    public string ContatoNome { get; set; } // NOVO: Adicionado para o frontend
    public IReadOnlyCollection<MessageDto> Mensagens { get; set; } = new List<MessageDto>();

    // Informações do Atendimento Atual
    public Guid? AtendimentoId { get; set; }
    public Guid? AgenteId { get; set; }
    public Guid? SetorId { get; set; }
    public string Status { get; set; }
    public string BotStatus { get; set; }

    public bool SessaoWhatsappAtiva { get; set; }
    public DateTime? SessaoWhatsappExpiraEm { get; set; } // Data de expiração da sessão ativa, se houver
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
