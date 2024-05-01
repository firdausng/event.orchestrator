namespace app.core.Options;

public class PrometheusOptions
{
    public static readonly string SectionName = "Prometheus";
    public required string MetricsPath { get; set; }
    public required int LocalPort { get; set; }
}