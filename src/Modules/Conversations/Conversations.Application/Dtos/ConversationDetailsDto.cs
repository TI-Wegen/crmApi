using System.Text.Json.Serialization;

namespace Conversations.Application.Dtos;

public class ConversationDetailsDto
{
    public Guid Id { get; set; }
    public Guid ContatoId { get; set; }
    public string ContatoNome { get; set; } = string.Empty;
    public string ContatoTelefone { get; set; } = string.Empty;
    public IReadOnlyCollection<MessageDto> Mensagens { get; set; } = new List<MessageDto>();
    public Guid? AtendimentoId { get; set; }
    public Guid? AgenteId { get; set; }
    public Guid? SetorId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? TagId { get; set; }
    public string BotStatus { get; set; } = string.Empty;
    public bool SessaoWhatsappAtiva { get; set; }
    public DateTime? SessaoWhatsappExpiraEm { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    [JsonIgnore]
    public bool HasPreviousPage => CurrentPage > 1;
    
    [JsonIgnore]
    public bool HasNextPage => CurrentPage < TotalPages;
}

public record MessageDto
{
    public Guid Id { get; init; }
    public string Texto { get; init; } = string.Empty;
    public string? AnexoUrl { get; init; }
    public DateTime Timestamp { get; init; }
    public string RemetenteTipo { get; init; } = string.Empty;
    public Guid? RemetenteAgenteId { get; init; }
    public string? Wamid { get; set; }
    public string? ReacaoMensagem { get; set; }
}

public record MessageWithConversationIdDto : MessageDto
{
    public string ConversationId { get; set; } = default!;
}
