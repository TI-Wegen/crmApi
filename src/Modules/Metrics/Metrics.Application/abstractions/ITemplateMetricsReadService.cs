using Metrics.Application.Dtos;
using Metrics.Domain.Entities;

namespace Metrics.Application.abstractions
{
    public interface ITemplateMetricsReadService
    {
        Task<IEnumerable<TemplatesSentPerAgentDto>> GetSentCountPerAgentAsync(DateTime startDate, DateTime endDate);
            Task AddTemplateSentMetricAsync(MetricaTemplateEnviado metricaTemplateEnviado);
    }

}
