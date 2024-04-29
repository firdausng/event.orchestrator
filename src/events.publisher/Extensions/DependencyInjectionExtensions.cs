using app.core.Infrastructure.Kafka;
using app.core.Infrastructure.Kafka.Options;
using events.publisher.Commands;

namespace events.publisher.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddSingleton<PublishEventListCommand>();
        return services;
    }
    
    public static IServiceCollection AddKafkaProducer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaProducerService(configuration);
        return services;
    }

    public static IHealthChecksBuilder AddAppHealthChecks(this IHealthChecksBuilder healthChecksBuilder, IConfiguration configuration)
    {
        healthChecksBuilder.AddKafka(producerConfig =>
            {
                producerConfig.BootstrapServers = configuration.GetSection(KafkaOptions.SectionName)
                    .GetValue<string>(nameof(KafkaOptions.BootstrapServers));
                // producerConfig.SaslMechanism = configuration.GetSection(KafkaOptions.SectionName)
                //     .GetValue<SaslMechanism>(nameof(KafkaOptions.SaslMechanism));

            });
        return healthChecksBuilder;
    }
}