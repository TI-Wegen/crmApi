namespace CRM.Infrastructure.Config.Meta;

public interface IMessageDeduplicationService
{
    Task<bool> TryRegisterMessageAsync(string messageId, TimeSpan ttl);
}