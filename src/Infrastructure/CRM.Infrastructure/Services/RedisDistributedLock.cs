using CRM.Application.Interfaces;
using StackExchange.Redis;

namespace CRM.Infrastructure.Services;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    private readonly string _lockOwnerId = Guid.NewGuid().ToString();

    public RedisDistributedLock(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public Task<bool> AcquireLockAsync(string resourceKey, TimeSpan expiry)
    {
        return _database.LockTakeAsync(resourceKey, _lockOwnerId, expiry);
    }

    public Task ReleaseLockAsync(string resourceKey)
    {
        return _database.LockReleaseAsync(resourceKey, _lockOwnerId);
    }
}