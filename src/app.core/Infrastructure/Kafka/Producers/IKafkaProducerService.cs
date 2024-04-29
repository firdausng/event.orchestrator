using CloudNative.CloudEvents;

namespace app.core.Infrastructure.Kafka.Producers;

public interface IKafkaProducerService
{
    Task ProduceAsync(CloudEvent message, string? partitionKey = null);
}