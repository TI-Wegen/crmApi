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
        // Aqui também seria o local ideal para disparar os eventos de domínio
        // antes ou depois de salvar as alterações.
        return _context.SaveChangesAsync(cancellationToken);
    }
}


