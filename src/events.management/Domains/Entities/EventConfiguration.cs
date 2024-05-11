using System.Text.Json;

namespace events.management.Domains.Entities;

public class EventConfiguration : AggregateRoot
{
    // this only for ef core to work
    // do use this constructor
    private EventConfiguration(){}
    private EventConfiguration(EventConfigurationId id, string eventName, Destination destination)
    {
        Id = id;
        EventName = eventName;
        Destination = destination;
    }
    public EventConfigurationId Id { get; private set; }
    public string EventName { get; private set; }
    public Destination Destination { get; private set; }
    
    public static EventConfiguration Create(string eventName, Destination destination)
    {
        var dest = new EventConfiguration(new EventConfigurationId(Guid.NewGuid()), eventName, destination);
        return dest;
    }

    public override string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
}