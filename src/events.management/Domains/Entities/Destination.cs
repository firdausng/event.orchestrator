namespace events.management.Domains.Entities;

public abstract class Destination(DestinationId id, DestinationType type) : AggregateRoot
{
    public DestinationId Id { get; private set; } = id;
    public DestinationType Type { get; private set; } = type;
}