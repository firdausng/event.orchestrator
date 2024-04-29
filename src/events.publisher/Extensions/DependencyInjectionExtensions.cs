using app.core.Infrastructure.Kafka;
using app.core.Infrastructure.Kafka.Options;
using app.core.Monitoring;
using events.publisher.Commands;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

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
    
    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMonitoringService(configuration);
        services.AddOpenTelemetry()
            .WithTracing(providerBuilder => providerBuilder
                .AddAspNetCoreInstrumentation())
            .WithMetrics(providerBuilder =>
            {
                providerBuilder
                    .AddAspNetCoreInstrumentation()
                    // .AddRuntimeInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting","Microsoft.AspNetCore.Server.Kestrel")
                    .AddView("http.server.request.duration",
                        new ExplicitBucketHistogramConfiguration
                        {
                            Boundaries =
                            [
                                0, 0.005, 0.01, 0.025, 0.05,
                                0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10
                            ]
                        })
                    // https://github.com/open-telemetry/opentelemetry-dotnet/issues/5502
                    .AddPrometheusExporter(o => o.DisableTotalNameSuffixForCounters = true);
            });
        return services;
    }
}