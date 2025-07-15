using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.Domain.DomainEvents;

public abstract class Entity
{
    public Guid Id { get; protected set; }
    public Guid Version { get; protected set; }
    public DateTime CreatedAt { get; protected set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// Interface marcadora para os eventos
public interface IDomainEvent { }