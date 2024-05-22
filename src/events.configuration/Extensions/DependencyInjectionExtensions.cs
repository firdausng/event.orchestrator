using app.core.Data.CloudEvents;
using app.core.Infrastructure.Kafka;
using app.core.Infrastructure.Kafka.Options;
using app.core.Monitoring;
using app.core.Options;
using events.configuration.Data;
using events.configuration.Handlers;
using events.configuration.Monitoring;
using events.configuration.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace events.configuration.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKafkaConsumerWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaConsumerService(configuration);

        services.AddOptions<CloudEventHandlerOptions>().Bind(configuration.GetSection(CloudEventHandlerOptions.SectionName));
        services.AddSingleton<ICloudEventConsumerHandler, EventManagementHandler>();
        services.AddHttpClient();
        return services;
    }
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        // services.AddTransient<CreateEventConfigurationCommand>();
        return services;
    }
    
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var migrationsAssembly = typeof(DependencyInjectionExtensions).Assembly.GetName().Name;
        
        services.AddDbContext<EventConfigurationDbContext>(cfg =>
        {
            cfg
                .UseNpgsql(configuration.GetConnectionString("AppConnectionString"),
                    options =>
                    {
                        options.EnableRetryOnFailure(3);
                        options.MigrationsAssembly(migrationsAssembly);
                    })
                .LogTo(Console.WriteLine, LogLevel.Information)
                //.EnableSensitiveDataLogging()
                ;
        });
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
    
    public static IServiceCollection AddAppOpenTelemetry(this IServiceCollection services, IConfiguration configuration,
        ILoggingBuilder builderLogging)
    {
        services.AddMonitoringService(configuration);
        
        builderLogging.AddOpenTelemetry(opt =>
        {
            opt.AddOtlpExporter();
        });
        
        services.AddOpenTelemetry()
            .ConfigureResource(builder =>
            {
                builder.AddService(DiagnosticsConfig.ServiceName)
                    .AddAttributes(new List<KeyValuePair<string, object>>
                    {
                        new("app-region", "localhost")
                    });
            })
            .WithTracing(providerBuilder => providerBuilder
                .AddSource(DiagnosticsConfig.ServiceName)
                .ConfigureResource(resource => resource.AddService(DiagnosticsConfig.ServiceName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317");})
            )
            .WithMetrics(providerBuilder =>
            {
                providerBuilder
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(DiagnosticsConfig.Meter.Name)
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
                    .AddPrometheusExporter(o => o.DisableTotalNameSuffixForCounters = true)
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri("http://localhost:4317");
                        
                    });
            });
        return services;
    }
    
    public static IApplicationBuilder UseAppOpenTelemetryPrometheus(this IApplicationBuilder app)
    {
        var prometheusOptions = app.ApplicationServices.GetRequiredService<IOptions<PrometheusOptions>>().Value;
        app.UseOpenTelemetryPrometheusScrapingEndpoint(
            context => context.Request.Path == prometheusOptions.MetricsPath
                       && context.Connection.LocalPort == prometheusOptions.LocalPort);
        return app;
    }
}