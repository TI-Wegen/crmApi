namespace CRM.Domain.DomainEvents;

public interface IDomainEventDispatcher
{
    Task DispatchAndClearEvents(IEnumerable<Entity> entitiesWithEvents);
}