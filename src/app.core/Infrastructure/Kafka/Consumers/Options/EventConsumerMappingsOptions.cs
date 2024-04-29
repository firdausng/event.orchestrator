﻿using app.core.Infrastructure.Kafka.Options;

namespace app.core.Infrastructure.Kafka.Consumers.Options;

public class EventConsumerMappingsOptions
{
    public static readonly string SectionName = "EventConsumerMappings";
    public string[] Topics { get; set; }
}

public class KafkaConsumerOptions
{
    public static readonly string SectionName = "KafkaConsumer";
    public required string GroupId { get; set; }
}