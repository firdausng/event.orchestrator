using System.Text.Json.Serialization;

namespace events.management.Models;

public record CreateEventConfigurationRequest
{
    public required string Name { get; set; }
    public required string Id { get; set; }
    public string? Group { get; set; }
    public EventDestinationRequest Destination { get; set; }
}

[JsonDerivedType(typeof(EventDestinationRequest), typeDiscriminator: "base")]
[JsonDerivedType(typeof(WebhookDestinationRequest), typeDiscriminator: "webhook")]
[JsonDerivedType(typeof(GoogleDestinationRequest), typeDiscriminator: "google")]
public record EventDestinationRequest
{
}

public record WebhookDestinationRequest: EventDestinationRequest
{
    public required Uri Url { get; set; }
    public required string ContentType { get; set; }
    
}

public record GoogleDestinationRequest: EventDestinationRequest
{
    public required string Query { get; set; }
    
}