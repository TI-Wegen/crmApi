using CRM.Application.Dto;
using CRM.Application.Interfaces;
using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Tags.Domain.repository;

namespace Tags.Infrastructure.Repository;

public class TagRepository : ITagRepository
{
    private readonly AppDbContext _context;
    private readonly IUserContext _userContext;
    
    public TagRepository(AppDbContext context, IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task AddAsync(Domain.Aggregates.Tags tags, CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tags, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Domain.Aggregates.Tags?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Domain.Aggregates.Tags?> GetByNameAsync(string name,
        CancellationToken cancellationToken = default)
    {
        return await _context.Tags.FirstOrDefaultAsync(x => x.Nome == name, cancellationToken);
    }

    public async Task<PagedResult<Domain.Aggregates.Tags>> GetAllAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Tags.CountAsync(cancellationToken);
        var currentLoggedUser = _userContext.GetCurrentUserId();

        var items = await _context.Tags
            .Where(x => x.AgenteId == currentLoggedUser)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Domain.Aggregates.Tags>(items, totalCount);
    }

    public async Task UpdateAsync(Domain.Aggregates.Tags tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await GetByIdAsync(id, cancellationToken);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}