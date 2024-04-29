using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace app.core.Infrastructure.Kafka;

public class KafkaStatisticsHandler<TKey, TValue>
{
    private readonly ILogger<KafkaStatisticsHandler<TKey, TValue>> _logger;

    public KafkaStatisticsHandler(ILogger<KafkaStatisticsHandler<TKey, TValue>> logger)
    {
        _logger = logger;
    }
    
    public Action<IProducer<TKey, TValue>, string> SetProducerStatisticsHandler()
    {
        return (producer, s) => { _logger.LogInformation(s); };
    }
    
    public Action<IConsumer<TKey, TValue>, string> SetConsumerStatisticsHandler()
    {
        return (producer, s) => { _logger.LogInformation(s); };
    }
}