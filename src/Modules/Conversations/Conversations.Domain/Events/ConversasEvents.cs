using CRM.Domain.DomainEvents;

public record ConversaIniciadaEvent(Guid ConversaId, Guid ContatoId, DateTime Timestamp) : IDomainEvent;

public record ConversaResolvidaEvent(Guid ConversaId, Guid? AgenteId, DateTime Timestamp) : IDomainEvent;

public record ConversaAtribuidaEvent(Guid ConversaId, Guid AgenteId, DateTime Timestamp) : IDomainEvent;

public record ConversaFinalizadaEvent(Guid ConversaId, Guid? AgenteId, DateTime Timestamp) : IDomainEvent;

public record MensagemAdicionadaEvent(Guid ConversaId, Guid MensagemId, string Texto, DateTime Timestamp)
    : IDomainEvent;

public record ConversaTransferidaEvent(Guid ConversaId, Guid NovoAgenteId, Guid NovoSetorId) : IDomainEvent;

public record SessaoDaConversaExpirada(Guid ConversaId) : IDomainEvent;

public record ConversaReabertaEvent(Guid ConversaId, DateTime Timestamp) : IDomainEvent;

public record ConversaExpiradaEvent(Guid ConversaId, DateTime Timestamp) : IDomainEvent;