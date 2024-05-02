using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace events.subscriber.Monitoring;

public static class DiagnosticsConfig
{
    public const string ServiceName = "Subscriber";
    public static Meter Meter = new(ServiceName);
    public static Counter<long> WebhookCount = Meter.CreateCounter<long>("webhook.count");
    public static ActivitySource Source = new ActivitySource(ServiceName);
}