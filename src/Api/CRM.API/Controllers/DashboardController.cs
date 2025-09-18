using CRM.API.Controllers.Base;
using Dashboard.Application.UseCases.Queries;
using Microsoft.AspNetCore.Mvc;
using CRM.Application.Interfaces;
using Dashboard.Domain.Dtos;

namespace CRM.API.Controllers;

public class DashboardController : BaseController
{
    private readonly IQueryHandler<DashboardFullQuery, DashboardFullResponseQuery> _dashboardFullHandler;

    private readonly IQueryHandler<DashboardPersonalQuery, DashboardPersonalResponseQuery>
        _dashboardPersonalResponseQuery;

    public DashboardController(IQueryHandler<DashboardFullQuery, DashboardFullResponseQuery> dashboardFullHandler,
        IQueryHandler<DashboardPersonalQuery, DashboardPersonalResponseQuery> dashboardPersonalResponseQuery)
    {
        _dashboardFullHandler = dashboardFullHandler;
        _dashboardPersonalResponseQuery = dashboardPersonalResponseQuery;
    }

    [HttpGet("Full")]
    public async Task<IActionResult> GetFullDashbboard()
    {
        try
        {
            var response = await _dashboardFullHandler.HandleAsync(new DashboardFullQuery());
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"--> Erro crítico ao processar: {ex.Message} \n {ex.StackTrace}");
            return NoContent();
        }
    }

    [HttpGet("{id:guid}/Personal")]
    public async Task<IActionResult> GetPersonalDashbboard(DashboardPersonalQuery dashboardPersonalQuery)
    {
        try
        {
            var response = await _dashboardPersonalResponseQuery.HandleAsync(dashboardPersonalQuery);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"--> Erro crítico ao processar: {ex.Message} \n {ex.StackTrace}");
            return NoContent();
        }
    }
}