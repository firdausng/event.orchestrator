namespace app.core.Options;

public class PrometheusOptions
{
    public static readonly string SectionName = "Prometheus";
    public string MetricsPath { get; set; }
    public int LocalPort { get; set; }
}