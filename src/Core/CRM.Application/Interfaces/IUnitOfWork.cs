namespace CRM.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ReloadEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;

}
