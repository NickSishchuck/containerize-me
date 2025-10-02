using EventLogger.Models;

namespace EventLogger.Services;

public class EventGeneratorService : IEventGenerator
{
    private readonly Random _random = new();
    private readonly string[] _eventTypes =
    {
        "UserLogin",
        "OrderProcessed",
        "DataSync",
        "PaymentReceived",
        "EmailSent",
        "CacheRefresh",
        "BackupCompleted",
        "APIRequest"
    };

    private readonly string[] _severities = { "Info", "Info", "Info", "Warning", "Error" };

    public EventData GenerateRandomEvent()
    {
        var eventType = _eventTypes[_random.Next(_eventTypes.Length)];
        var severity = _severities[_random.Next(_severities.Length)];

        return new EventData
        {
            EventType = eventType,
            Message = GenerateMessageForEvent(eventType),
            Timestamp = DateTime.UtcNow,
            Severity = severity,
            Metadata = new Dictionary<string, object>
            {
                { "ProcessId", Environment.ProcessId },
                { "MachineName", Environment.MachineName },
                { "ThreadId", Environment.CurrentManagedThreadId },
                { "RandomId", _random.Next(1000, 9999) }
            }
        };
    }

    private string GenerateMessageForEvent(string eventType)
    {
        return eventType switch
        {
            "UserLogin" => $"User user{_random.Next(1, 100)} logged in successfully",
            "OrderProcessed" => $"Order #{_random.Next(1000, 9999)} processed",
            "DataSync" => $"Synchronized {_random.Next(10, 500)} records",
            "PaymentReceived" => $"Payment of ${_random.Next(10, 1000)} received",
            "EmailSent" => $"Email sent to customer{_random.Next(1, 50)}@example.com",
            "CacheRefresh" => $"Cache refreshed for {_random.Next(5, 50)} keys",
            "BackupCompleted" => $"Backup completed: {_random.Next(100, 500)}MB",
            "APIRequest" => $"API endpoint /{GetRandomEndpoint()} called",
            _ => "Unknown event occurred"
        };
    }

    private string GetRandomEndpoint()
    {
        string[] endpoints = { "users", "orders", "products", "payments", "reports" };
        return endpoints[_random.Next(endpoints.Length)];
    }
}
