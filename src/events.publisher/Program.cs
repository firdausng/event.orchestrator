using app.core.Monitoring;
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
builder.Services.AddAppOpenTelemetry(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/healthz");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAppOpenTelemetryPrometheus();

app.MapControllers();
app.Run();