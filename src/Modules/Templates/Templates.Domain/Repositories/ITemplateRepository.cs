using Templates.Domain.Aggregates;

namespace Templates.Domain.Repositories
{
    public interface ITemplateRepository
    {
        Task AddAsync(MessageTemplate template, CancellationToken cancellationToken = default);
        Task<MessageTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<MessageTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    }
}
