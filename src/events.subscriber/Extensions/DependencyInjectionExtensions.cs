using app.core.Data.CloudEvents;
using app.core.Infrastructure.Kafka;
using app.core.Monitoring;
using app.core.Options;
using events.subscriber.Options;
using events.subscriber.Services;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Prometheus;

namespace events.subscriber.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddKafkaConsumerWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddKafkaConsumerService(configuration);

        services.AddOptions<CloudEventHandlerOptions>().Bind(configuration.GetSection(CloudEventHandlerOptions.SectionName));
        services.AddSingleton<ICloudEventConsumerHandler, CloudEventConsumerHandler>();
        services.AddHttpClient();
        return services;
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
                    .AddHttpClientInstrumentation()
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
                    ;
            });
        return services;
    }
    
    public static IApplicationBuilder UseAppPrometheus(this IApplicationBuilder app)
    {
        var prometheusOptions = app.ApplicationServices.GetRequiredService<IOptions<PrometheusOptions>>().Value;
        app.UseMetricServer(prometheusOptions.LocalPort,opt =>
        {
            opt.EnableOpenMetrics = true;
        }, prometheusOptions.MetricsPath); 
        app.UseHttpMetrics(); 
        // app.UseOpenTelemetryPrometheusScrapingEndpoint(
        //     context => context.Request.Path == prometheusOptions.MetricsPath
        //                && context.Connection.LocalPort == prometheusOptions.LocalPort);
        return app;
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