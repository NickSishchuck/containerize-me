using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventLogger.Services;

public class EventProcessor : IEventProcessor
{
    private readonly IEventGenerator _eventGenerator;
    private readonly ILogger<EventProcessor> _logger;
    private readonly IConfiguration _configuration;

    public EventProcessor(
        IEventGenerator eventGenerator,
        ILogger<EventProcessor> logger,
        IConfiguration configuration)
    {
        _eventGenerator = eventGenerator;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        var iterations = _configuration.GetValue<int>("EventProcessor:Iterations", 20);
        var delayMs = _configuration.GetValue<int>("EventProcessor:DelayMs", 1000);

        _logger.LogInformation("Event Processor started. Iterations: {Iterations}, Delay: {DelayMs}ms",
            iterations, delayMs);

        for (int i = 1; i <= iterations && !cancellationToken.IsCancellationRequested; i++)
        {
            var eventData = _eventGenerator.GenerateRandomEvent();

            LogEvent(eventData, i, iterations);

            if (i < iterations)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        _logger.LogInformation("Event Processor completed all {Iterations} iterations", iterations);
    }

    private void LogEvent(Models.EventData eventData, int currentIteration, int totalIterations)
    {
        var logMessage = "[{Iteration}/{Total}] {EventType}: {Message}";
        var args = new object[]
        {
            currentIteration,
            totalIterations,
            eventData.EventType,
            eventData.Message
        };

        switch (eventData.Severity)
        {
            case "Error":
                _logger.LogError(logMessage, args);
                break;
            case "Warning":
                _logger.LogWarning(logMessage, args);
                break;
            default:
                _logger.LogInformation(logMessage, args);
                break;
        }

        foreach (var metadata in eventData.Metadata)
        {
            _logger.LogDebug("Metadata: {Key}={Value}", metadata.Key, metadata.Value);
        }
    }
}
