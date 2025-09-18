using CRM.Application.Interfaces;
using Dashboard.Domain.Dtos;


namespace Dashboard.Application.UseCases.Queries;

public record DashboardFullQuery() : IQuery<DashboardFullResponseQuery?>;