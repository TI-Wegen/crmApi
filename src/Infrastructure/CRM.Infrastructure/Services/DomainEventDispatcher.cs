using CRM.Domain.DomainEvents;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Infrastructure.Services;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    // Em vez de injetar o MediatR, injetamos o provedor de serviços do .NET
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
                // Para cada evento, usamos o IServiceProvider para encontrar todos os
                // handlers registrados que implementam a nossa interface IDomainEventHandler<TEvent>.
                var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
                var handlers = _serviceProvider.GetServices(handlerType);

                foreach (var handler in handlers)
                {
                    if (handler is null) continue;

                    // Usamos reflexão para chamar o método 'Handle' do handler.
                    var method = handler.GetType().GetMethod("Handle");
                    await (Task)method.Invoke(handler, new object[] { domainEvent, CancellationToken.None });
                }
            }
        }
    }
}
