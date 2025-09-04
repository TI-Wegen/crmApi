using CRM.Application.Dto;

namespace Tags.Domain.repository;

public interface ITagRepository
{
    Task AddAsync(Aggregates.Tags tags, CancellationToken cancellationToken = default);
    Task<Aggregates.Tags?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Aggregates.Tags?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<PagedResult<Domain.Aggregates.Tags>> GetAllAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Aggregates.Tags agente, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}