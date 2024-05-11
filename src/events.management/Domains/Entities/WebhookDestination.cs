using System.Text.Json;

namespace events.management.Domains.Entities;

public class WebhookDestination: Destination
{
    private WebhookDestination(DestinationId id, Uri url, string contentType): base(id, new DestinationType("webhook"))
    {
        Url = url;
        ContentType = contentType;
    }
    public Uri Url { get; private set; }
    public string ContentType { get; private set; }

    public static WebhookDestination Create(Uri url, string contentType)
    {
        var dest = new WebhookDestination(new DestinationId(Guid.NewGuid()), url, contentType);
        return dest;
    }
    
    public override string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }
}