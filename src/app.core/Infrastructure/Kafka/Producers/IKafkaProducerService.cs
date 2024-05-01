using CloudNative.CloudEvents;

namespace app.core.Infrastructure.Kafka.Producers;

public interface IKafkaProducerService
{
    Task<bool> ProduceAsync(CloudEvent message, string? partitionKey = null);
}