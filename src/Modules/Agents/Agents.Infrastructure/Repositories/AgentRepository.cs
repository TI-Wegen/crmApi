using Agents.Domain.Aggregates;
using Agents.Domain.Enuns;
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

    public async Task<IEnumerable<Agente>> GetAllAsync(int pageNumber, int pageSize, bool incluirInativos, CancellationToken cancellationToken = default)
    {
        var query = _context.Agentes.AsQueryable();

        if (!incluirInativos)
        {
            query = query.Where(a => a.Status != AgenteStatus.Inativo);
        }

        return await query
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
    public async Task<Setor?> GetSetorByNomeAsync(string nome, CancellationToken cancellationToken = default)
    {
        return await _context.Setores
            .FirstOrDefaultAsync(s => s.Nome == nome, cancellationToken);
    }

    public Task<Setor?> GetSetorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Setores.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Setor>> GetSetoresAsync(CancellationToken cancellationToken = default)
    {
        var query = _context.Setores.AsQueryable();

        return await query
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<Agente?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Agentes
            .FirstOrDefaultAsync(a => a.Nome == name, cancellationToken);
    }
}