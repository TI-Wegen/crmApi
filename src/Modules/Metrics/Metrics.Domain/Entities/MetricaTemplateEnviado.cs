namespace Metrics.Domain.Entities;

public class MetricaTemplateEnviado
{
    public Guid Id { get; set; }
    public Guid AtendimentoId { get; set; }
    public Guid AgenteId { get; set; }
    public string TemplateName { get; set; }
    public DateTime SentAt { get; set; }
}
