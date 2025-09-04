namespace Conversations.Application.Dtos;

public class ConversationDetailsDto
{
    public Guid Id { get; set; }
    public Guid ContatoId { get; set; }
    public string ContatoNome { get; set; }
    public string ContatoTelefone { get; set; }
    public IReadOnlyCollection<MessageDto> Mensagens { get; set; } = new List<MessageDto>();
    public Guid? AtendimentoId { get; set; }
    public Guid? AgenteId { get; set; }
    public Guid? SetorId { get; set; }
    public string Status { get; set; }
    public Guid? TagId { get; set; }
    public string BotStatus { get; set; }
    public bool SessaoWhatsappAtiva { get; set; }
    public DateTime? SessaoWhatsappExpiraEm { get; set; }
}

public record MessageDto
{
    public Guid Id { get; init; }
    public string Texto { get; init; }
    public string? AnexoUrl { get; init; }
    public DateTime Timestamp { get; init; }
    public string RemetenteTipo { get; init; }
    public Guid? RemetenteAgenteId { get; init; }
    public string? Wamid { get; set; }
}

public record MessageWithConversationIdDto : MessageDto
{
    public string ConversationId { get; set; } = default!;
}