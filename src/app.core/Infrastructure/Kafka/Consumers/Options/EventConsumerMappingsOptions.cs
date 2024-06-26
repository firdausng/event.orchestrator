﻿namespace app.core.Infrastructure.Kafka.Consumers.Options;

public class EventConsumerMappingsOptions
{
    public static readonly string SectionName = "EventConsumerMappings";
    public required string[] Topics { get; set; }
    
}

public class KafkaConsumerOptions
{
    public static readonly string SectionName = "KafkaConsumer";
    public required string GroupId { get; set; }
    public required bool AllowAutoCreateTopics { get; set; }
    public required bool EnableAutoCommit { get; set; }
    public required double SubscriberIntervalInSeconds { get; set; }
    public required int BatchSize { get; set; }
    public required double BatchTimeoutInSec { get; set; }
    public void Deconstruct(out string groupId, out bool allowAutoCreateTopics, out bool enableAutoCommit)
    {
        groupId = GroupId;
        allowAutoCreateTopics = AllowAutoCreateTopics;
        enableAutoCommit = EnableAutoCommit;
    }
}