using CRM.Application.Interfaces;
using Dashboard.Application.Dtos;
using Dashboard.Application.Repository;

namespace Dashboard.Application.UseCases.Queries.Handler;

public class DashboardFullQueryHandler(IDashboardRepository dashboardRepository)
    : IQueryHandler<DashboardFullQuery, DashboardFullResponseQuery?>
{
    public async Task<DashboardFullResponseQuery?> HandleAsync(DashboardFullQuery query,
        CancellationToken cancellationToken = default)
    {
        return await dashboardRepository.GetFullDashboardAsync(cancellationToken);
    }
}