using Dashboard.Application.Dtos;

namespace Dashboard.Application.Repository;

public interface IDashboardRepository
{
    Task<DashboardFullResponseQuery?> GetFullDashboardAsync(CancellationToken cancellationToken = default);
    Task<DashboardPersonalResponseQuery?> GetProfileDashboardAsync(Guid id, CancellationToken cancellationToken = default);
}