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
            await HandleEvents2(stoppingToken);
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
    
    private async Task HandleEvents2(CancellationToken stoppingToken)
    {
        var buffer = new List<ConsumeResult<string?, byte[]>>();
        var bufferLock = new SemaphoreSlim(1,1);
        _logger.LogInformation("start processing with max batch size {BatchSize}", _kafkaConsumerOptions.BatchSize);

        var count = 0;
        _ = Task.Run(async () =>
        {
            var batchCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            batchCts.CancelAfter(TimeSpan.FromSeconds(_kafkaConsumerOptions.BatchTimeoutInSec));
            while (!stoppingToken.IsCancellationRequested)
            {
                count = buffer.Count;
                if (count < _kafkaConsumerOptions.BatchSize && !batchCts.Token.IsCancellationRequested)
                {
                    continue;   
                }

                try
                {
                    await bufferLock.WaitAsync(stoppingToken);
                    if (buffer.Count > 0)
                    {
                        await HandleEventBatch(buffer);
                        buffer.Clear();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Error handling the batch");
                }
                finally
                {
                    batchCts.Dispose();
                    bufferLock.Release();
                    batchCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    batchCts.CancelAfter(TimeSpan.FromSeconds(_kafkaConsumerOptions.BatchTimeoutInSec));
                }
                
            }
            
            _logger.LogInformation("handling events is stopped");
            if (buffer.Count > 0)
            {
                _logger.LogInformation("buffer count ({Count}) is more than zero, start process remaining events", buffer.Count);
                await HandleEventBatch(buffer);
                buffer.Clear();
                batchCts.Dispose();
            }
            
        }, stoppingToken);
        await foreach (var consumeResult in _kafkaConsumerChannel.Reader.ReadAllAsync(stoppingToken))
        {
            await bufferLock.WaitAsync(stoppingToken);
            try
            {
                buffer.Add(consumeResult);
            }
            finally
            {
                bufferLock.Release();
            }
        }
    }

    private async Task HandleEvents(CancellationToken stoppingToken)
    {
        var buffer = new List<ConsumeResult<string?, byte[]>>();
        var batchCts = new CancellationTokenSource(TimeSpan.FromSeconds(_kafkaConsumerOptions.BatchTimeoutInSec));
        _logger.LogInformation("start processing with max batch size {BatchSize}", _kafkaConsumerOptions.BatchSize);
        await foreach (var consumeResult in _kafkaConsumerChannel.Reader.ReadAllAsync(stoppingToken))
        {
            buffer.Add(consumeResult);
            _logger.LogInformation("start processing {OffsetValue} events with batch size {} and buffer count {}", 
                consumeResult.Offset.Value, _kafkaConsumerOptions.BatchSize, buffer.Count);
            if (buffer.Count < _kafkaConsumerOptions.BatchSize && !batchCts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("buffer count ({Count}) still lower that max batch size ({BatchSize}) and set to {ProcessOrNot} or Timeout have not been reach {TimeoutReach}", 
                    buffer.Count, _kafkaConsumerOptions.BatchSize, buffer.Count < _kafkaConsumerOptions.BatchSize, !batchCts.Token.IsCancellationRequested);
                continue;
            }
            await HandleEventBatch(buffer);
            _logger.LogInformation("done processing {OffsetValue} events with batch size {} and buffer count {}", 
                consumeResult.Offset.Value, _kafkaConsumerOptions.BatchSize, buffer.Count);
            buffer.Clear();
            batchCts.Dispose();
            batchCts = new CancellationTokenSource(TimeSpan.FromSeconds(_kafkaConsumerOptions.BatchTimeoutInSec));
        }
        
        
        _logger.LogInformation("loop end");
        if (buffer.Count > 0)
        {
            _logger.LogInformation("buffer count ({Count}) is more than zero, start process remaining events", buffer.Count);
            await HandleEventBatch(buffer);
            buffer.Clear();
            batchCts.Dispose();
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
        var lastOffset = batch.Last();
        var allOffset = batch.Select(o => o.Offset.Value);
        var allOffsetStr = string.Join(',', allOffset);
        _logger.LogInformation("Done processing {Count} events, storing offset at {Offset}, the list of offset are {AllOffsetStr}", 
            tasks.Count, lastOffset.Offset.Value, allOffsetStr);
        _consumer.StoreOffset(lastOffset);
    }

    /// <inheritdoc />
    public override void Dispose()
    {
        _logger.LogInformation("Kafka Consumer Worker is shutting down");
        _consumer.Dispose();  
        base.Dispose();
    }
}