using System.Linq.Expressions;
using Agents.Application.Repositories;
using Agents.Domain.Aggregates;
using Agents.Domain.Entities;
using Agents.Domain.Enuns;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Agents.Infrastructure.Adapters;

public class AgentAdapter : IAgentRepository
{
    private readonly AppDbContext _context;

    public AgentAdapter(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Agente agente, CancellationToken cancellationToken = default)
    {
        await _context.Agentes.AddAsync(agente, cancellationToken);
    }

    public async Task<IEnumerable<Agente>> FilterAsync(int pageNumber, int pageSize, bool incluirInativos,
        Expression<Func<Agente, bool>>? condition = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Agentes.AsQueryable();

        if (!incluirInativos)
        {
            query = query.Where(a => a.Status != AgenteStatus.Inativo);
        }

        if (condition != null)
        {
            query = query.Where(condition);
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }


    public Task<Agente?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Agentes.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
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
}