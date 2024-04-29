using app.core.Data.CloudEvents;
using app.core.Infrastructure.Kafka;
using events.subscriber.Options;
using events.subscriber.Services;
using events.subscriber.Workers;

namespace events.subscriber.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<KafkaConsumerWorker>();
        services.AddOptions<CloudEventHandlerOptions>().Bind(configuration.GetSection(CloudEventHandlerOptions.SectionName));
        services.AddSingleton<ICloudEventConsumerHandler, CloudEventConsumerHandler>();
        services.AddHttpClient();
        // services.AddHostedService<TestConsumerWorker>();
        return services;
    }
    
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaConsumerService(configuration);

        
        // services
        //     .AddHealthChecksUI()
        //     .AddInMemoryStorage();
        return services;
    }
    
}