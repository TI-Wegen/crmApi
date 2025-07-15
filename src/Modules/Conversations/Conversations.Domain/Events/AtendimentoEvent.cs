using CRM.Domain.DomainEvents;

namespace Conversations.Domain.Events;

    public class AtendimentoEvent
{
    public record AtendimentoIniciadoEvent(Guid AtendimentoId, Guid ConversaId) : IDomainEvent;

    public record AtendimentoAtribuidoEvent(Guid AtendimentoId, Guid AgenteId, DateTime Timestamp) : IDomainEvent;

    public record AtendimentoResolvidoEvent(Guid AtendimentoId, Guid? AgenteId, DateTime Timestamp) : IDomainEvent;

    public record AtendimentoReabertoEvent(Guid AtendimentoId, DateTime Timestamp) : IDomainEvent;
    public record AtendimentoFinalizadoEvent(Guid AtendimentoId, Guid? AgenteId, DateTime Timestamp) : IDomainEvent;
    public record AtendimentoTransferidoEvent(Guid AtendimentoId, Guid NovoAgenteId, Guid NovoSetorId) : IDomainEvent;
    public record AtendimentoExpiradoEvent(Guid AtendimentoId) : IDomainEvent;
}

