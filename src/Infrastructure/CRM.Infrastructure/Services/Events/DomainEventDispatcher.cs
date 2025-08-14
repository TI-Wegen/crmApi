using CRM.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Services.Events;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAndClearEvents(IEnumerable<Entity> entitiesWithEvents)
    {
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToArray();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = _serviceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    if (handler is null) continue;

                    var method = handler.GetType().GetMethod("Handle");
                    await (Task)method.Invoke(handler, new object[] { domainEvent, CancellationToken.None });
                }
            }
        }
    }
}
