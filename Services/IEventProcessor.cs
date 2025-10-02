namespace EventLogger.Services;

public interface IEventProcessor
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
