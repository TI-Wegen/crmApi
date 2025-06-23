using Agents.Domain.Aggregates;
using Agents.Domain.Repository;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Agents.Infrastructure.Repositories;



public class AgentRepository : IAgentRepository
{
    private readonly AppDbContext _context;

    public AgentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Agente agente, CancellationToken cancellationToken = default)
    {
        await _context.Agentes.AddAsync(agente, cancellationToken);
    }

    public Task<Agente?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _context.Agentes.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
    }

    public Task<Agente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Agentes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Agente>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Ordenar é importante para que a paginação seja consistente.
        // Skip e Take são os comandos do LINQ que o EF Core traduz para a paginação em SQL.
        return await _context.Agentes
            .OrderBy(a => a.Nome)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Agente agente, CancellationToken cancellationToken = default)
    {
        _context.Agentes.Update(agente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}