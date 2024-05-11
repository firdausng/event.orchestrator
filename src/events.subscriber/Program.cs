using events.subscriber.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddKafkaConsumerWorker(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddAppOpenTelemetry(builder.Configuration, builder.Logging);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Docker")
{
}
app.UseAppOpenTelemetryPrometheus();
app.MapHealthChecks("/healthz");
app.MapGet("/", () =>"events.subscriber");

app.Run();
