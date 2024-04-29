using events.subscriber.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddWorkers();
builder.Services.AddKafkaConsumer(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddOpenTelemetry()
    .WithTracing(providerBuilder => providerBuilder
        .AddAspNetCoreInstrumentation())
    .WithMetrics(providerBuilder =>
    {
        providerBuilder
            .AddAspNetCoreInstrumentation()
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
            .AddPrometheusExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   
}
app.UseOpenTelemetryPrometheusScrapingEndpoint(
    context => context.Request.Path == "/internal/metrics"
               && context.Connection.LocalPort == 9091);
app.MapHealthChecks("/healthz");
app.MapGet("/", () =>"events.subscriber");
app.Run();
