using CRM.Infrastructure.Config.Meta;
using StackExchange.Redis;
using System.Text.Json;

namespace CRM.Infrastructure.Services;

    public class RedisMessageBufferService : IMessageBufferService
{
    private readonly IDatabase _database;
    private const string BufferKeyPrefix = "buffer:contato:";
    private const string ProcessorKeyPrefix = "processor:contato:";
    private static readonly TimeSpan BufferExpiry = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan ProcessorLockExpiry = TimeSpan.FromSeconds(30);

    public RedisMessageBufferService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task AddToBufferAsync(string contactPhone, MessageObject message)
    {
        var key = $"{BufferKeyPrefix}{contactPhone}";
        var serializedMessage = JsonSerializer.Serialize(message);
        await _database.ListRightPushAsync(key, serializedMessage);
        await _database.KeyExpireAsync(key, BufferExpiry);
    }

    public Task<bool> IsFirstProcessor(string contactPhone)
    {
        var key = $"{ProcessorKeyPrefix}{contactPhone}";
 
        return _database.StringSetAsync(key, "active", ProcessorLockExpiry, When.NotExists);
    }

    public async Task<IEnumerable<MessageObject>> ConsumeBufferAsync(string contactPhone)
    {
        var bufferKey = $"{BufferKeyPrefix}{contactPhone}";
        var processorKey = $"{ProcessorKeyPrefix}{contactPhone}";

        var redisValues = await _database.ListRangeAsync(bufferKey);

        await _database.KeyDeleteAsync(new RedisKey[] { bufferKey, processorKey });

        return redisValues.Select(val => JsonSerializer.Deserialize<MessageObject>(val));
    }
}

