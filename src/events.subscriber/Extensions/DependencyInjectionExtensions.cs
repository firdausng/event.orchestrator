using app.core.Infrastructure.Kafka;
using events.subscriber.Workers;

namespace events.subscriber.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<KafkaConsumerWorker>();
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