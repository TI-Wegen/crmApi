using Conversations.Application.Repositories;

namespace Conversations.Infrastructure.Services;

using StackExchange.Redis;
using System.Text.Json;

public class RedisBotSessionCache : IBotSessionCache
{
    private readonly IDatabase _database;
    private const string KeyPrefix = "botsession:";

    public RedisBotSessionCache(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task SetStateAsync(string contactPhone, BotSessionState state, TimeSpan expiry)
    {
        var key = $"{KeyPrefix}{contactPhone}";
        var value = JsonSerializer.Serialize(state);
        await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<BotSessionState?> GetStateAsync(string contactPhone)
    {
        var key = $"{KeyPrefix}{contactPhone}";
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty) return null;

        return JsonSerializer.Deserialize<BotSessionState>(value);
    }

    public async Task DeleteStateAsync(string contactPhone)
    {
        var key = $"{KeyPrefix}{contactPhone}";
        await _database.KeyDeleteAsync(key);
    }
}