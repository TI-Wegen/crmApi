using CRM.Application.Dto;

namespace Tags.Application.repositories;

public interface ITagRepository
{
    Task AddAsync(Domain.Entities.Tags tags, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Tags?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Domain.Entities.Tags?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<PagedResult<Domain.Entities.Tags>> GetAllAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Domain.Entities.Tags agente, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}