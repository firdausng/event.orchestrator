using app.core.Infrastructure.Kafka.Producers.Options;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace app.core.Infrastructure.Kafka.Producers;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly KafkaCloudNativeMessageService _kafkaCloudNativeMessageService;
    private readonly IProducer<string?, byte[]> _producer;
    private readonly EventProducerMappingsOptions _eventProducerMappingsOptionsVal;

    public KafkaProducerService(
        KafkaCloudNativeMessageService kafkaCloudNativeMessageService, 
        KafkaProducerBuilderHandler<string?, byte[]> builderHandler,
        IOptions<EventProducerMappingsOptions> eventProducerMappingsOptions
        )
    {
        _kafkaCloudNativeMessageService = kafkaCloudNativeMessageService;
        _producer = builderHandler.Build();
        _eventProducerMappingsOptionsVal = eventProducerMappingsOptions.Value;
    }

    public async Task<bool> ProduceAsync(CloudEvent message, string? partitionKey)
    {
        var msg = _kafkaCloudNativeMessageService.ToKafkaMessage(message, ContentMode.Binary, new JsonEventFormatter());
        
        if (message.Type is null)
        {
            //TODO need to handle when event name cannot be found
            return false;
        }
        var eventProducerConfiguration = _eventProducerMappingsOptionsVal.Configurations[message.Type];
        
        if (partitionKey is not null)
        {
            msg.Key = partitionKey;
        }
        
        await _producer.ProduceAsync(eventProducerConfiguration.Topic, msg);
        return true;
    }
    
    public void Dispose()
    {
        _producer.Flush();
        _producer.Dispose();
    }
}