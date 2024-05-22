using events.management.Data;
using events.management.Domains.Entities;
using events.management.Models;

namespace events.management.Commands;

public class CreateEventConfigurationCommand
{
    private readonly EventsManagementDbContext _dbContext;

    public CreateEventConfigurationCommand(EventsManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task Handle(CreateEventConfigurationRequest request)
    {
        // var cloudEvent = new CloudEvent();
        // cloudEvent.Id = request.Id;
        // cloudEvent.Type = "test.event";
        // cloudEvent.Source = new Uri("/api/Publish", UriKind.Relative);
        // // cloudEvent.Data = request.Data;
        //
        // using var activity = DiagnosticsConfig.Source.StartActivity(DiagnosticsConfig.Source.Name, ActivityKind.Producer);
        // activity?.SetTag("request.Id", request.Id);
        // cloudEvent["traceparent"] = activity.Id;
        // cloudEvent["tracestate"] = activity.TraceStateString;
        // var result = await _kafkaProducerService.ProduceAsync(cloudEvent, request.Group);
        //
        // // instrumentation
        // var labels = new KeyValuePair<string, object?>("event", request.Id);
        // DiagnosticsConfig.PublishCount.Add(1, labels);

        var entity = EventConfiguration.Create(
            request.Name,
            WebhookDestination.Create(new Uri("https://google.com"), "application/json")
            );
        
        _dbContext.EventConfigurations.Add(entity);
        entity.RaiseDomainEvent(new EventConfigurationCreated(entity));
        await _dbContext.SaveChangesAsync();
    }
}