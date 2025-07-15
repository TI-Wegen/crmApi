using CRM.Application.Interfaces;
using StackExchange.Redis;

namespace CRM.Infrastructure.Services;

public class RedisDistributedLock : IDistributedLock
{
    private readonly IDatabase _database;
    // Um valor único para identificar quem possui a trava.
    private readonly string _lockOwnerId = Guid.NewGuid().ToString();

    public RedisDistributedLock(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public Task<bool> AcquireLockAsync(string resourceKey, TimeSpan expiry)
    {
        // O comando LockTakeAsync do StackExchange.Redis é atômico.
        // Ele tenta definir a chave apenas se ela não existir.
        return _database.LockTakeAsync(resourceKey, _lockOwnerId, expiry);
    }

    public Task ReleaseLockAsync(string resourceKey)
    {
        // Libera a trava, permitindo que outro processo a adquira.
        return _database.LockReleaseAsync(resourceKey, _lockOwnerId);
    }
}

