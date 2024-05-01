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
    private readonly KafkaConsumerOptions _kafkaConsumerOptions;
    private readonly KafkaConsumerChannel<string?, byte[]> _kafkaConsumerChannel;
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly ICloudEventConsumerHandler _eventConsumerHandler;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string?, byte[]> _consumer;
    private readonly EventConsumerMappingsOptions _eventConsumerMappingsOptions;

    public KafkaConsumerWorker(
        KafkaCloudNativeMessageService kafkaCloudNativeMessageService, 
        KafkaConsumerBuilderHandler<string?, byte[]> builderHandler, 
        IOptions<EventConsumerMappingsOptions> eventConsumerMappingOptions,
        IOptions<KafkaConsumerOptions> kafkaConsumerOptions,
        KafkaConsumerChannel<string?, byte[]> kafkaConsumerChannel,
        ILogger<KafkaConsumerWorker> logger,
        ICloudEventConsumerHandler eventConsumerHandler,
        IServiceProvider serviceProvider
        )
    {   
        _kafkaCloudNativeMessageService = kafkaCloudNativeMessageService;
        _kafkaConsumerOptions = kafkaConsumerOptions.Value;
        _kafkaConsumerChannel = kafkaConsumerChannel;
        _logger = logger;
        _eventConsumerHandler = eventConsumerHandler;
        _serviceProvider = serviceProvider;
        _eventConsumerMappingsOptions = eventConsumerMappingOptions.Value;
        _consumer = builderHandler.Build();
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Kafka consumer worker");
        Task.Run(async () =>
        {
            await ConsumeAsync(stoppingToken);
        }, stoppingToken);

        Task.Run(async () =>
        {
            await HandleEvents(stoppingToken);
        }, stoppingToken);
        return Task.CompletedTask;
    }

    private async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        try 
        {
            _logger.LogInformation("Starting subscribing to Kafka consumer with these topic {Topics}", string.Join(',', _eventConsumerMappingsOptions.Topics));
            _consumer.Subscribe(_eventConsumerMappingsOptions.Topics);
            var maxBatchReleaseTime = TimeSpan.FromSeconds(_kafkaConsumerOptions.SubscriberIntervalInSeconds);
            while (!stoppingToken.IsCancellationRequested)
            {
                var batchStart = DateTime.UtcNow;
                var availableTime = maxBatchReleaseTime - (DateTime.UtcNow - batchStart);

                while (availableTime > TimeSpan.Zero)
                {
                    try
                    {
                        var consumeResult = _consumer.Consume(availableTime);
                        if (consumeResult == null) continue;
                        if (!consumeResult.IsPartitionEOF)
                        {
                            await _kafkaConsumerChannel.Writer.WriteAsync(consumeResult, stoppingToken);
                        }
                        else
                        {
                            _logger.LogInformation("Reached end of {Topic} / {Partition} / {Offset}",
                                consumeResult.Topic, consumeResult.Partition, consumeResult.Offset);
                        }
                        availableTime = maxBatchReleaseTime - (DateTime.UtcNow - batchStart);
                    }
                    catch (ConsumeException e)
                    {
                        _logger.LogError(e, $"Consume error");
                    }
                }
            }
        }
        catch(Exception e)
        {
            _logger.LogError(e, "An error occurred in ExecuteAsync");
            throw;
        }
    }

    private async Task HandleEvents(CancellationToken stoppingToken)
    {
        var buffer = new List<ConsumeResult<string?, byte[]>>();
        var batchCts = new CancellationTokenSource(TimeSpan.FromSeconds(_kafkaConsumerOptions.BatchTimeoutInSec));
        
        await foreach (var consumeResult in _kafkaConsumerChannel.Reader.ReadAllAsync(stoppingToken))
        {
            buffer.Add(consumeResult);
            if (buffer.Count < _kafkaConsumerOptions.BatchSize && !batchCts.Token.IsCancellationRequested) continue;
            await HandleEventBatch(buffer);
            buffer.Clear();
            batchCts.Dispose();
            batchCts = new CancellationTokenSource(TimeSpan.FromSeconds(_kafkaConsumerOptions.BatchTimeoutInSec));
        }

        if (buffer.Count > 0)
        {
            await HandleEventBatch(buffer);
            buffer.Clear();
        }
    }

    private async Task HandleEventBatch(List<ConsumeResult<string?, byte[]>> batch)
    {
        var tasks = batch
            .Select(consumeResult =>
                _kafkaCloudNativeMessageService
                    .ToCloudEvent(consumeResult.Message, new JsonEventFormatter(), null))
            .Select(cloudEvent => _eventConsumerHandler.HandleAsync(cloudEvent))
            .ToList();

        await Task.WhenAll(tasks);
        _logger.LogInformation("Done processing {Count} events", tasks.Count());
        _consumer.StoreOffset(batch.Last());
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _logger.LogInformation("Kafka Consumer Worker is shutting down");
        _consumer.Dispose();  
        base.Dispose();
    }
}