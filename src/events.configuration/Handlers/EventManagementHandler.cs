using System.Diagnostics;
using System.Text.Json;
using app.core.Data.CloudEvents;
using CloudNative.CloudEvents;
using events.configuration.Data;
using events.configuration.Domains.Entities;
using events.configuration.Monitoring;
using events.management.core.Domains.Entities;

namespace events.configuration.Handlers;

public class EventManagementHandler: ICloudEventConsumerHandler
{
    private readonly EventConfigurationDbContext _eventConfigurationDbContext;

    public EventManagementHandler(EventConfigurationDbContext eventConfigurationDbContext )
    {
        _eventConfigurationDbContext = eventConfigurationDbContext;
    }
    public async Task<CloudEventStatus> HandleAsync(CloudEvent payload)
    {
        var activitySource = new ActivitySource(DiagnosticsConfig.ServiceName);
       
        ActivityContext activityContext = default;
        if (payload["traceparent"] is string traceparent)
        {
            var traceState = payload["tracestate"] as string;
            activityContext = ActivityContext.Parse(traceparent, traceState);
        }
        
        using var activity = activitySource.StartActivity(DiagnosticsConfig.Source.Name, ActivityKind.Consumer, activityContext);
        activity?.AddTag("cloudEventId", payload.Id);

        var str = JsonSerializer.Serialize(payload.Data);
        var outboxMessage = JsonSerializer.Deserialize<OutboxMessage>(str);

        switch (outboxMessage?.ClrType)
        {
            case null:
                break;
            case "events.management.Domains.Entities.EventConfiguration":
                break;
            case "events.management.Domains.Entities.WebhookDestination":
                var configuration = JsonSerializer.Deserialize<Destination>(outboxMessage.Content);
                // if (configuration is not null)
                // {
                //     switch (configuration.Type.Value)
                //     {
                //         case "webhook":
                //         {
                //             var webhookConfiguration = WebhookConfiguration.Create(outboxMessage.EventName,
                //                 new Uri(configuration.Url), configuration.ContentType);
                //             break;
                //         }
                //     }
                //     
                //     _eventConfigurationDbContext.Configurations.Add(webhookConfiguration);
                //     await _eventConfigurationDbContext.SaveChangesAsync();
                //     return new CloudEventStatus(true, "Successfully saved to db");
                // }

                break;
        }
        
        return new CloudEventStatus(false, "Error processing event");
    }
}