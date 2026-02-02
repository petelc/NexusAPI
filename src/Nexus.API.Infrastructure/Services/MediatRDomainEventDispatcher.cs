using MediatR;
using Traxs.SharedKernel;

namespace Nexus.API.Infrastructure.Services;

/// <summary>
/// MediatR implementation of domain event dispatcher
/// Publishes domain events through MediatR pipeline
/// </summary>
public class MediatRDomainEventDispatcher : IDomainEventDispatcher
{
  private readonly IMediator _mediator;

  public MediatRDomainEventDispatcher(IMediator mediator)
  {
    _mediator = mediator;
  }

  public async Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entitiesWithEvents)
  {
    foreach (var entity in entitiesWithEvents)
    {
      var events = entity.DomainEvents.ToArray();
      entity.ClearDomainEvents();

      foreach (var domainEvent in events)
      {
        await _mediator.Publish(domainEvent).ConfigureAwait(false);
      }
    }
  }

}
