using CRM.Application.Interfaces;
using Dashboard.Application.Dtos;
using Dashboard.Application.Repository;

namespace Dashboard.Application.UseCases.Queries.Handler;

public class DashboardPersonalQueryHandler : IQueryHandler<DashboardPersonalQuery, DashboardPersonalResponseQuery>
{
    private readonly IDashboardRepository _dashboardRepository;

    public DashboardPersonalQueryHandler(IDashboardRepository dashboardRepository)
    {
        _dashboardRepository = dashboardRepository;
    }

    public Task<DashboardPersonalResponseQuery> HandleAsync(DashboardPersonalQuery query,
        CancellationToken cancellationToken = default)
    {
        return _dashboardRepository.GetProfileDashboardAsync(query.Id);
    }
}