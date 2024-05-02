using System.Diagnostics;
using app.core.Infrastructure.Kafka.Producers;
using CloudNative.CloudEvents;
using events.publisher.Models;
using events.publisher.Monitoring;

namespace events.publisher.Commands;

public class PublishEventListCommand
{
    private readonly IKafkaProducerService _kafkaProducerService;

    public PublishEventListCommand(IKafkaProducerService kafkaProducerService)
    {
        _kafkaProducerService = kafkaProducerService;
    }
    public async Task Handle(PublishEventRequest request)
    {
        var cloudEvent = new CloudEvent();
        cloudEvent.Id = request.Id;
        cloudEvent.Type = "test.event";
        cloudEvent.Source = new Uri("/api/Publish", UriKind.Relative);
        cloudEvent.Data = request.Data;
        
        using var activity = DiagnosticsConfig.Source.StartActivity(DiagnosticsConfig.Source.Name, ActivityKind.Producer);
        activity?.SetTag("request.Id", request.Id);
        cloudEvent["traceparent"] = activity.Id;
        cloudEvent["tracestate"] = activity.TraceStateString;
        await _kafkaProducerService.ProduceAsync(cloudEvent, request.Group);
        
        // instrumentation
        var labels = new KeyValuePair<string, object?>("event", request.Id);
        DiagnosticsConfig.PublishCount.Add(1, labels);
    }
}