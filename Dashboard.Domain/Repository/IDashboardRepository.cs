using Dashboard.Domain.Dtos;

namespace Dashboard.Domain.Repository;

public interface IDashboardRepository
{
    Task<DashboardFullResponseQuery?> GetFullDashboardAsync(CancellationToken cancellationToken = default);
    Task<DashboardPersonalResponseQuery?> GetProfileDashboardAsync(Guid id, CancellationToken cancellationToken = default);
}