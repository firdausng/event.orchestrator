namespace app.core.Infrastructure.Kafka.Producers.Options;

public class EventProducerMappingsOptions
{
    public static readonly string SectionName = "EventProducerMappings";
    public Dictionary<string, EventProducerConfiguration> Configurations { get; set; }
}

public class EventProducerConfiguration
{
    public required string Topic { get; set; }
    public required string Name { get; set; }
    public required string GroupName { get; set; }
}

public class EventProducerGroupOptions
{
    public static readonly string SectionName = "EventProducerGroup";
    public EventProducerGroup[] EventProducerGroups { get; set; }
}

public class EventProducerGroup
{
    public required string Name { get; set; }
}

public class KafkaProducerOptions
{
    public static readonly string SectionName = "KafkaProducer";
}