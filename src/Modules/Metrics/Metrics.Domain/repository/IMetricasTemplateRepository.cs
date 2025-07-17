using Metrics.Domain.Entities;

namespace Metrics.Domain.repository
{
    public interface IMetricasTemplateRepository
    {
        Task AddTemplateSentMetricAsync(MetricaTemplateEnviado metricaTemplateEnviado);
        Task<IEnumerable<MetricaTemplateEnviado>> GetAllMetricsAsync();
        Task<MetricaTemplateEnviado> GetMetricByIdAsync(Guid id);
        Task<IEnumerable<MetricaTemplateEnviado>> GetMetricsByAtendimentoIdAsync(Guid atendimentoId);
    }

}
