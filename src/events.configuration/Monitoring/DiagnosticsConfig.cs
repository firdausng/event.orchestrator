using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace events.configuration.Monitoring;

public static class DiagnosticsConfig
{
    public const string ServiceName = "EventsConfiguration";
    public static Meter Meter = new(ServiceName);
    public static Counter<long> PublishCount = Meter.CreateCounter<long>("event.count");
    public static ActivitySource Source = new(ServiceName);
}