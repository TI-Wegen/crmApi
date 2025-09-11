namespace Conversations.Application.Dtos;

public class AtendimentoDto
{
    public Guid Id { get; init; }
    public Guid ConversaId { get; init; }
    public Guid? AgenteId { get; init; }
    public Guid? SetorId { get; init; }
    public string Status { get; set; }
    public string BotStatus { get; init; }
    public DateTime? DataFinalizacao { get; init; }
    public AvaliacaoDto? Avaliacao { get; init; }
}

public record AvaliacaoDto
{
    public int Nota { get; init; }
    public string? Comentario { get; init; }
}