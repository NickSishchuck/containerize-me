namespace EventLogger.Models;

public class EventData
{
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "Info";
    public Dictionary<string, object> Metadata { get; set; } = new();
}
