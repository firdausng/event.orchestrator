using System.Text.Json;
using System.Text.Json.Serialization;

namespace events.management.worker.Models;

public class PublishConfigurationRequest
{
    public required string Id { get; set; }
    public required EventData Data { get; set; }
}

public class EventData
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> DynamicData { get; set; }
}