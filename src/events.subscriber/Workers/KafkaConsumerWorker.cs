using app.core.Infrastructure.Kafka.Consumers;

namespace events.subscriber.Workers;

public sealed class KafkaConsumerWorker: BackgroundService
{
    private readonly ILogger<KafkaConsumerWorker> _logger;
    private readonly IKafkaConsumerService _kafkaConsumerService;

    public KafkaConsumerWorker(ILogger<KafkaConsumerWorker> logger, IKafkaConsumerService kafkaConsumerService)
    {
        _logger = logger;
        _kafkaConsumerService = kafkaConsumerService;
    }   
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        =>  Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);

    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start consuming message");
        await _kafkaConsumerService.ConsumeAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _logger.LogInformation("Worker is shutting down");
        _kafkaConsumerService.Dispose();
        base.Dispose();
    }
}

public sealed class TestConsumerWorker : BackgroundService
{
    private readonly ILogger<TestConsumerWorker> _logger;

    public TestConsumerWorker(ILogger<TestConsumerWorker> logger)
    {
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Start consuming message");
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Start consuming message");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}