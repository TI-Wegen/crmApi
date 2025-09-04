using Agents.Domain.Aggregates;

namespace Agents.Domain.Repository;

public interface IAgentRepository
{
    Task AddAsync(Agente agente, CancellationToken cancellationToken = default);
    Task<Agente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Agente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Agente>> GetAllAsync(int pageNumber, int pageSize, bool incluirInativos, CancellationToken cancellationToken = default);
    Task<Setor?> GetSetorByNomeAsync(string nome, CancellationToken cancellationToken = default);
    Task<Setor?> GetSetorByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Agente agente, CancellationToken cancellationToken = default);
    Task <IEnumerable <Setor>>  GetSetoresAsync(CancellationToken cancellationToken = default);
}