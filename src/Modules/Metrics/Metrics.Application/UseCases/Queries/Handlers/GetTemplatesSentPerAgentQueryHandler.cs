using CRM.Application.Interfaces;
using Metrics.Application.abstractions;
using Metrics.Application.Dtos;

namespace Metrics.Application.UseCases.Queries.Handlers;

public class GetTemplatesSentPerAgentQueryHandler
    : IQueryHandler<GetTemplatesSentPerAgentQuery, IEnumerable<TemplatesSentPerAgentDto>>
{
    private readonly ITemplateMetricsReadService _readService;

    public GetTemplatesSentPerAgentQueryHandler(ITemplateMetricsReadService readService)
    {
        _readService = readService;
    }

    public async Task<IEnumerable<TemplatesSentPerAgentDto>> HandleAsync(
        GetTemplatesSentPerAgentQuery query,
        CancellationToken cancellationToken)
    {
        return await _readService.GetSentCountPerAgentAsync(query.StartDate, query.EndDate);
    }
}