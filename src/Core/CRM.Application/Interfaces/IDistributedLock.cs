namespace CRM.Application.Interfaces
{
    public interface IDistributedLock
    {
        Task<bool> AcquireLockAsync(string resourceKey, TimeSpan expiry);

        Task ReleaseLockAsync(string resourceKey);
    }
}