using CRM.Application.Interfaces;
using Dashboard.Domain.Dtos;

namespace Dashboard.Application.UseCases.Queries;

public record DashboardPersonalQuery(
    Guid Id
    ) : IQuery<DashboardPersonalResponseQuery>;