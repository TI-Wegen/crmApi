namespace Conversations.Application.Dtos;

public record ActiveChatDto
{
    public AtendimentoDto AtendimentoAtual { get; init; }
    public AtendimentoDto? AtendimentoAnterior { get; init; }
    public IReadOnlyCollection<MessageDto> Mensagens { get; init; }
}
