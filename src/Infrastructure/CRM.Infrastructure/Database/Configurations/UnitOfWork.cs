using CRM.Application.Interfaces;

namespace CRM.Infrastructure.Database.Configurations;

 public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task ReloadEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class
    {
        return _context.Entry(entity).ReloadAsync(cancellationToken);
    }
}


