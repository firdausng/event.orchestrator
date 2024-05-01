using System.Text.Json;
using System.Text.Json.Serialization;

namespace events.publisher.Models;

public class EventData
{
    public EventData(Dictionary<string, JsonElement> dynamicData)
    {
        DynamicData = dynamicData;
    }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> DynamicData { get; set; }
}

public class PublishEventRequest
{
    public required EventData Data { get; set; }
    public required string Name { get; set; }
    public required string Id { get; set; }
    public string? Group { get; set; }
}