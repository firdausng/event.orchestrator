using Confluent.Kafka;

namespace app.core.Infrastructure.Kafka.Options;

public class KafkaOptions
{
    public static readonly string SectionName = "Kafka";
    public required string BootstrapServers { get; set; }
}

