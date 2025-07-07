namespace Conversations.Application.Abstractions;

using Conversations.Application.Dtos;
using Conversations.Domain.Enuns;

public interface IBotSessionCache
{
    Task SetStateAsync(string contactPhone, BotSessionState state, TimeSpan expiry);
    Task<BotSessionState?> GetStateAsync(string contactPhone);
    Task DeleteStateAsync(string contactPhone);
}

public record BotSessionState(
    Guid ConversationId,
    BotStatus Status,
    List<BoletoDto>? BoletosDisponiveis = null
);