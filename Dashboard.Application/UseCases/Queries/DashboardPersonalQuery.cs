using CRM.Application.Interfaces;
using Dashboard.Application.Dtos;

namespace Dashboard.Application.UseCases.Queries;

public record DashboardPersonalQuery(
    Guid Id
    ) : IQuery<DashboardPersonalResponseQuery>;