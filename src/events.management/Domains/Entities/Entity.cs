using events.management.Domains.Interfaces;

namespace events.management.Domains.Entities;

public abstract class Entity
{
    public abstract string Serialize();
}

public abstract class AggregateRoot: Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}