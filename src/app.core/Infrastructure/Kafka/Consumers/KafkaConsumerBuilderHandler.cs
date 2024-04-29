using app.core.Infrastructure.Kafka.Consumers.Options;
using app.core.Infrastructure.Kafka.Options;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace app.core.Infrastructure.Kafka.Consumers;

public class KafkaConsumerBuilderHandler<TKey,TValue>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerBuilderHandler<TKey, TValue>> _logger;
    private readonly ConsumerBuilder<TKey, TValue> _consumer;

    public KafkaConsumerBuilderHandler(
        IOptions<KafkaOptions> kafkaOptions, 
        IOptions<KafkaConsumerOptions> kafkaConsumerOptions,
        IServiceProvider serviceProvider, 
        ILogger<KafkaConsumerBuilderHandler<TKey,TValue>> logger
        )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        var kafkaCoreOptions = kafkaOptions.Value;
        var kafkaConsumerOptVal = kafkaConsumerOptions.Value;
        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaCoreOptions.BootstrapServers,
            GroupId = kafkaConsumerOptVal.GroupId,
            AutoOffsetReset = AutoOffsetReset.Latest,
            AllowAutoCreateTopics = kafkaConsumerOptVal.AllowAutoCreateTopics
        };
        _consumer = new ConsumerBuilder<TKey,TValue>(config);
    }
    
    public IConsumer<TKey,TValue> Build()
    {
        var statisticsHandler = _serviceProvider.GetService<KafkaStatisticsHandler<TKey,TValue>>();
        _consumer.SetStatisticsHandler(statisticsHandler?.SetConsumerStatisticsHandler());
        return _consumer.Build();
    }
}