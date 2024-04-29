using app.core.Infrastructure.Kafka.Consumers;
using app.core.Infrastructure.Kafka.Consumers.Options;
using app.core.Infrastructure.Kafka.Options;
using app.core.Infrastructure.Kafka.Producers;
using app.core.Infrastructure.Kafka.Producers.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace app.core.Infrastructure.Kafka;

public static class KafkaDependencyInjectionExtension
{
    public static IServiceCollection AddKafkaCoreService(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<KafkaCloudNativeMessageService>();
        services.AddOptions<KafkaOptions>()
            .Bind(configuration.GetSection(KafkaOptions.SectionName));
        services.AddSingleton(typeof(KafkaStatisticsHandler<,>));
        return services;
    }
    
    public static IServiceCollection AddKafkaProducerService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaCoreService(configuration);
        
        services.AddOptions<EventProducerMappingsOptions>().Bind(configuration.GetSection(EventProducerMappingsOptions.SectionName));
        services.AddOptions<EventProducerGroupOptions>().Bind(configuration.GetSection(EventProducerGroupOptions.SectionName));
        services.AddOptions<KafkaProducerOptions>().Bind(configuration.GetSection(KafkaProducerOptions.SectionName));
        services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
        services.AddSingleton(typeof(KafkaProducerBuilderHandler<,>));
        
        return services;
    }

    public static IServiceCollection AddKafkaConsumerService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddKafkaCoreService(configuration);
        
        services.AddOptions<EventConsumerMappingsOptions>().Bind(configuration.GetSection(EventConsumerMappingsOptions.SectionName));
        services.AddOptions<KafkaConsumerOptions>().Bind(configuration.GetSection(KafkaConsumerOptions.SectionName));
        services.AddSingleton(typeof(IKafkaConsumerService), typeof(KafkaConsumerService));
        services.AddSingleton(typeof(KafkaConsumerBuilderHandler<,>));
        services.AddSingleton(typeof(KafkaConsumerChannel<,>));
        return services;
    }
}