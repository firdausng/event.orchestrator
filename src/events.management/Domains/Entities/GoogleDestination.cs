using System.Text.Json;

namespace events.management.Domains.Entities;

public class GoogleDestination: Destination
{
    private GoogleDestination(DestinationId id, string query): base(id, new DestinationType("google"))
    {
        Query = query;
    }
    public string Query { get; private set; }

    public static GoogleDestination Create(string query)
    {
        var dest = new GoogleDestination(new DestinationId(Guid.NewGuid()), query);
        return dest;
    }

    public override string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
}