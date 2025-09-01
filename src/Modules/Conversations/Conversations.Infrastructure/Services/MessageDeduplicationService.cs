using CRM.Infrastructure.Config.Meta;
using StackExchange.Redis;

namespace Conversations.Infrastructure.Services;

public class RedisMessageDeduplicationService : IMessageDeduplicationService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisMessageDeduplicationService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> TryRegisterMessageAsync(string messageId, TimeSpan ttl)
    {
        if (string.IsNullOrWhiteSpace(messageId))
            return false;

        var db = _redis.GetDatabase();
        
        return await db.StringSetAsync(
            key: $"msg:{messageId}",
            value: "1",
            expiry: ttl,
            when: When.NotExists
        );
    }
}