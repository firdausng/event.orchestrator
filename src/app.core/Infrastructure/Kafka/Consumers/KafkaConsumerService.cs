using app.core.Infrastructure.Kafka.Consumers.Options;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace app.core.Infrastructure.Kafka.Consumers;

public class KafkaConsumerService: IKafkaConsumerService
{
    private readonly KafkaCloudNativeMessageService _kafkaCloudNativeMessageService;
    private readonly KafkaConsumerChannel<string?, byte[]> _kafkaConsumerChannel;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly IConsumer<string?, byte[]> _consumer;
    private readonly EventConsumerMappingsOptions _eventConsumerMappingsOptions;

    public KafkaConsumerService(
        KafkaCloudNativeMessageService kafkaCloudNativeMessageService, 
        KafkaConsumerBuilderHandler<string?, byte[]> builderHandler, 
        IOptions<EventConsumerMappingsOptions> options,
        KafkaConsumerChannel<string?, byte[]> kafkaConsumerChannel,
        ILogger<KafkaConsumerService> logger
        )
    {   
        _kafkaCloudNativeMessageService = kafkaCloudNativeMessageService;
        _kafkaConsumerChannel = kafkaConsumerChannel;
        _logger = logger;
        _eventConsumerMappingsOptions = options.Value;
        _consumer = builderHandler.Build();
    }

    public Task ConsumeAsync(CancellationToken token)
    {
        _consumer.Subscribe(_eventConsumerMappingsOptions.Topics);
        while (!token.IsCancellationRequested)
        {
            var consumeResult = _consumer.Consume(token);
            var cloudEvent = _kafkaCloudNativeMessageService.ToCloudEvent(consumeResult.Message, new JsonEventFormatter(), null);
            _logger.LogInformation("The cloud event is {EventId}", cloudEvent.Id);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _consumer.Dispose();                                                          
    }
}