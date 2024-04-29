using app.core.Infrastructure.Kafka.Options;
using app.core.Infrastructure.Kafka.Producers.Options;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace app.core.Infrastructure.Kafka.Producers;

public class KafkaProducerBuilderHandler<TKey,TValue>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaProducerBuilderHandler<TKey, TValue>> _logger;
    private readonly ProducerBuilder<TKey, TValue> _producer;

    public KafkaProducerBuilderHandler(
        IOptions<KafkaOptions> kafkaCoreOptions, 
        IOptions<KafkaProducerOptions> kafkaProducerOptions, 
        IServiceProvider serviceProvider, 
        ILogger<KafkaProducerBuilderHandler<TKey,TValue>> logger
        )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var producerCoreOptVal = kafkaCoreOptions.Value;
        var kafkaProducerOptionsVal = kafkaProducerOptions.Value;
        var config = new ProducerConfig
        {
            BootstrapServers = producerCoreOptVal.BootstrapServers,
        };
        _producer = new ProducerBuilder<TKey,TValue>(config);
    }
    public IProducer<TKey,TValue> Build()
    {
        var statisticsHandler = _serviceProvider.GetService<KafkaStatisticsHandler<TKey,TValue>>();
        _producer.SetStatisticsHandler(statisticsHandler?.SetProducerStatisticsHandler());
        return _producer.Build();
    }
    
}