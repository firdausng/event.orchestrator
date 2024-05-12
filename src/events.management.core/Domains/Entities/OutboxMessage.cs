namespace events.management.core.Domains.Entities;

public class OutboxMessage
{
    public int Id { get;  set; }
    public string Content { get;  set; }
    public DateTime CreatedAt { get;  set; }
    public bool Published { get;  set; }
    public string EntryState { get;  set; }
    public string? ClrType { get;  set; }
    public string? TraceParent { get;  set; }
    public string? TraceState { get;  set; }
}