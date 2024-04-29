
using System.Threading.Channels;
using Confluent.Kafka;

namespace app.core.Infrastructure.Kafka.Consumers;

public class KafkaConsumerChannel<TKey, TVal>
{
    private readonly Channel<ConsumeResult<TKey, TVal>> _channel =
        Channel.CreateUnbounded<ConsumeResult<TKey, TVal>>();

    public ChannelReader<ConsumeResult<TKey, TVal>> Reader => _channel.Reader;
    public ChannelWriter<ConsumeResult<TKey, TVal>> Writer => _channel.Writer;

}