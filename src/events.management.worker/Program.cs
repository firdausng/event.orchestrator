using events.management.worker.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHandlers();
builder.Services.AddKafkaProducer(builder.Configuration);
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddHealthChecks()
    .AddAppHealthChecks(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAppOpenTelemetry(builder.Configuration, builder.Logging);

var app = builder.Build();

app.UseAppOpenTelemetryPrometheus();
app.MapHealthChecks("/healthz");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


app.Run();