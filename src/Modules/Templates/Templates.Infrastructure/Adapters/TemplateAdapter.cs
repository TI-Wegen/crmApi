using CRM.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Templates.Application.Repositories;
using Templates.Domain.Entities;

namespace Templates.Infrastructure.Adapters;

public class TemplateAdapter : ITemplateRepository
{
    private readonly AppDbContext _context;

    public TemplateAdapter(AppDbContext context)
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