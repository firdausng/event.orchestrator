namespace events.configuration.Handlers;

public class Destination
{
    public DestinationType Type { get; set; }
}

public class DestinationType
{
    public string Value { get; set; }
}