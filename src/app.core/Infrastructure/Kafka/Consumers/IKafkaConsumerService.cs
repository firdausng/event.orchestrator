namespace app.core.Infrastructure.Kafka.Consumers;

public interface IKafkaConsumerService: IDisposable
{
    Task ConsumeAsync(CancellationToken token);
}