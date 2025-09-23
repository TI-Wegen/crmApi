using Conversations.Application.Dtos;
using Conversations.Domain.Enuns;

namespace Conversations.Application.Repositories;

public interface IBotSessionCache
{
    Task SetStateAsync(string contactPhone, BotSessionState state, TimeSpan expiry);
    Task<BotSessionState?> GetStateAsync(string contactPhone);
    Task DeleteStateAsync(string contactPhone);
}

public record BotSessionState(
    Guid AtendimentoId,
    BotStatus Status,
    DateTime LastActivityTimestamp,
    List<BoletoDto>? BoletosDisponiveis = null
);