using EventLogger.Models;

namespace EventLogger.Services;

public interface IEventGenerator
{
    EventData GenerateRandomEvent();
}
