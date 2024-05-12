using System.Diagnostics;
using app.core.Infrastructure.Kafka.Producers;
using app.core.Infrastructure.Kafka.Producers.Options;
using CloudNative.CloudEvents;
using events.management.core.Domains.Entities;
using events.management.worker.Models;
using events.management.worker.Monitoring;
using Microsoft.Extensions.Options;

namespace events.management.worker.Commands;

public class PublishConfigurationEventCommand
{
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly EventProducerMappingsOptions _eventProducerMappingsOptions;

    public PublishConfigurationEventCommand(IKafkaProducerService kafkaProducerService, IOptions<EventProducerMappingsOptions> eventProducerMappingsOptions)
    {
        _kafkaProducerService = kafkaProducerService;
        _eventProducerMappingsOptions = eventProducerMappingsOptions.Value;
    }
    
    public async Task Handle(OutboxMessage request)
    {
        var activitySource = new ActivitySource(DiagnosticsConfig.ServiceName);
       
        ActivityContext activityContext = default;
        if (request.TraceParent is { } traceparent)
        {
            var traceState = request.TraceState;
            activityContext = ActivityContext.Parse(traceparent, traceState);
        }
        
        
        var cloudEvent = new CloudEvent
        {
            Id = request.Id.ToString(),
            Type = "test.event",
            Source = new Uri("/outbox", UriKind.Relative),
            Data = request
        };

        using var activity = activitySource.StartActivity(DiagnosticsConfig.Source.Name, ActivityKind.Producer, activityContext);
        activity?.AddTag("cloudEventId", request.Id.ToString());
        
        cloudEvent["traceparent"] = activity?.Id;
        cloudEvent["tracestate"] = activity?.TraceStateString;
        var result = await _kafkaProducerService.ProduceAsync(cloudEvent);
        
        // instrumentation
        var labels = new KeyValuePair<string, object?>("event", request.Id);
        DiagnosticsConfig.PublishCount.Add(1, labels);
    }
}