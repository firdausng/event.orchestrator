using app.core.Infrastructure.Kafka.Producers;
using CloudNative.CloudEvents;
using events.publisher.Models;

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
        await _kafkaProducerService.ProduceAsync(cloudEvent, request.Group);
    }
}