using events.configuration.Domains.Interfaces;

namespace events.configuration.Domains.Entities;

public class Entity
{
    
}

public abstract class AggregateRoot: Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}

public abstract class Configuration(string eventName, ConfigurationId id): AggregateRoot
{
    public string EventName { get; private set; } = eventName;
    public ConfigurationId Id { get; private set; } = id;
}

public class WebhookConfiguration: Configuration
{
    private WebhookConfiguration(ConfigurationId id, string eventName, Uri url, string contentType): base(eventName, id)
    {
        Url = url;
        ContentType = contentType;
    }
    
    public Uri Url { get; private set; }
    public string ContentType { get; private set; }
    
    public static WebhookConfiguration Create(string eventName, Uri url, string contentType)
    {
        var dest = new WebhookConfiguration(new ConfigurationId(Guid.NewGuid()), eventName, url, contentType);
        return dest;
    }
}

public readonly record struct ConfigurationId(Guid Value);
