namespace CRM.Infrastructure.Config.Meta
{
    public interface IMessageBufferService
    {
        Task AddToBufferAsync(string contactPhone, MessageObject message);
        Task<bool> IsFirstProcessor(string contactPhone);
        Task<IEnumerable<MessageObject>> ConsumeBufferAsync(string contactPhone);
    }
}