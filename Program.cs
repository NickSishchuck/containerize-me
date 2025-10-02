using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using EventLogger.Services;

namespace EventLogger;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog();
        });

        services.AddTransient<IEventGenerator, EventGeneratorService>();
        services.AddTransient<IEventProcessor, EventProcessor>();

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var processor = serviceProvider.GetRequiredService<IEventProcessor>();

        try
        {
            logger.LogInformation("=== EventLogger Application Started ===");
            logger.LogInformation("Environment: {Environment}",
                Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production");

            await processor.RunAsync();

            logger.LogInformation("=== EventLogger Application Completed Successfully ===");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }
    }
}
