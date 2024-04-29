using events.publisher.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHandlers();
builder.Services.AddKafkaProducer(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddAppHealthChecks(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
                    Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05,
                        0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
                })
            .AddPrometheusExporter();
    });

var app = builder.Build();

app.MapHealthChecks("/healthz");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint(
    context => context.Request.Path == "/internal/metrics"
               && context.Connection.LocalPort == 9090);

app.MapControllers();
app.Run();