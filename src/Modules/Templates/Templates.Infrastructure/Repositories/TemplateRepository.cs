using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Templates.Domain.Aggregates;
using Templates.Domain.Repositories;

namespace Templates.Infrastructure.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly AppDbContext _context;

    public TemplateRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(MessageTemplate template, CancellationToken cancellationToken = default)
    {
        await _context.MessageTemplates.AddAsync(template, cancellationToken);
    }

    public async Task<IEnumerable<MessageTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MessageTemplates.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<MessageTemplate?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.MessageTemplates.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }
}