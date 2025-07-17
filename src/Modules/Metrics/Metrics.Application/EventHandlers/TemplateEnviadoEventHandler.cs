using Conversations.Domain.Events;
using CRM.Application.Interfaces;
using CRM.Domain.DomainEvents;
using Metrics.Application.abstractions;
using Metrics.Domain.Entities;
using Metrics.Domain.repository;

namespace Metrics.Application.EventHandlers;

public class TemplateEnviadoEventHandler : IDomainEventHandler<TemplateEnviadoEvent>
{
    private readonly ITemplateMetricsReadService _metricasRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TemplateEnviadoEventHandler(ITemplateMetricsReadService metricasRepository, IUnitOfWork unitOfWork)
    {
        _metricasRepository = metricasRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(TemplateEnviadoEvent domainEvent, CancellationToken cancellationToken)
    {
        var metrica = new MetricaTemplateEnviado
        {
            Id = Guid.NewGuid(),
            AtendimentoId = domainEvent.AtendimentoId,
            AgenteId = domainEvent.AgenteId,
            TemplateName = domainEvent.TemplateName,
            SentAt = domainEvent.SentAt
        };

        await _metricasRepository.AddTemplateSentMetricAsync(metrica);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}