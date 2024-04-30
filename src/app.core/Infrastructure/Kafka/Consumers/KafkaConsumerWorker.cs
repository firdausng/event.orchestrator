using app.core.Data.CloudEvents;
using app.core.Infrastructure.Kafka.Consumers.Options;
using CloudNative.CloudEvents.SystemTextJson;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace app.core.Infrastructure.Kafka.Consumers;

public class KafkaConsumerWorker: BackgroundService
{
    private readonly KafkaCloudNativeMessageService _kafkaCloudNativeMessageService;
    private readonly KafkaConsumerChannel<string?, byte[]> _kafkaConsumerChannel;
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly ICloudEventConsumerHandler _eventConsumerHandler;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string?, byte[]> _consumer;
    private readonly EventConsumerMappingsOptions _eventConsumerMappingsOptions;

    public KafkaConsumerWorker(
        KafkaCloudNativeMessageService kafkaCloudNativeMessageService, 
        KafkaConsumerBuilderHandler<string?, byte[]> builderHandler, 
        IOptions<EventConsumerMappingsOptions> options,
        KafkaConsumerChannel<string?, byte[]> kafkaConsumerChannel,
        ILogger<KafkaConsumerWorker> logger,
        ICloudEventConsumerHandler eventConsumerHandler,
        IServiceProvider serviceProvider
        )
    {   
        _kafkaCloudNativeMessageService = kafkaCloudNativeMessageService;
        _kafkaConsumerChannel = kafkaConsumerChannel;
        _logger = logger;
        _eventConsumerHandler = eventConsumerHandler;
        _serviceProvider = serviceProvider;
        _eventConsumerMappingsOptions = options.Value;
        _consumer = builderHandler.Build();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Kafka consumer worker");
        return Task.Run(async () =>
        {
            await ConsumeAsync(stoppingToken);
        }, stoppingToken);
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        try 
        {
            _logger.LogInformation("Starting subscribing to Kafka consumer with these topic {Topics}", string.Join(',', _eventConsumerMappingsOptions.Topics));
            _consumer.Subscribe(_eventConsumerMappingsOptions.Topics);
            while (!stoppingToken.IsCancellationRequested)
            {
                await HandleEvent(stoppingToken);
            }
        }
        catch(Exception e)
        {
            _logger.LogError(e, "An error occurred in ExecuteAsync");
            throw;
        }
    }
    
    private async Task HandleEvent(CancellationToken token)
    {
        var consumeResult = _consumer.Consume(token);
        var cloudEvent = _kafkaCloudNativeMessageService.ToCloudEvent(consumeResult.Message, new JsonEventFormatter(), null);
        _logger.LogInformation("The cloud event is {EventId}", cloudEvent.Id);
        await _eventConsumerHandler.HandleAsync(cloudEvent);
        // _consumer.Commit();
    }
    
    public override void Dispose()
    {
        _logger.LogInformation("Worker is shutting down");
        _consumer.Dispose();  
        base.Dispose();
    }
}