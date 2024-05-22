namespace events.configuration.Options;

public class CloudEventHandlerOptions
{
    public static readonly string SectionName = "CloudEventHandler";
    public required Uri Url { get; set; }
}