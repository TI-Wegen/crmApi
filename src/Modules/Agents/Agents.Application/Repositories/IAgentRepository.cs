using System.Linq.Expressions;
using Agents.Domain.Aggregates;

namespace Agents.Application.Repositories;

public interface IAgentRepository
{
    Task AddAsync(Agente agente, CancellationToken cancellationToken = default);

    Task<IEnumerable<Agente>> FilterAsync(int pageNumber, int pageSize, bool incluirInativos,
        Expression<Func<Agente, bool>>? condition = null,
        CancellationToken cancellationToken = default);

    Task<Agente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Setor?> GetSetorByNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<Setor?> GetSetorByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Agente agente, CancellationToken cancellationToken = default);
    Task<IEnumerable<Setor>> GetSetoresAsync(CancellationToken cancellationToken = default);
}