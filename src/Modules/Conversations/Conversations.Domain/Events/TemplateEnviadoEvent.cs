using CRM.Domain.DomainEvents;

namespace Conversations.Domain.Events;

public record TemplateEnviadoEvent(
    Guid AtendimentoId,
    Guid AgenteId,
    string TemplateName,
    DateTime SentAt
) : IDomainEvent;