using HotelSearch.Api.Configuration;

namespace HotelSearch.Api.Infrastructure;

internal static class StartupLogger
{
    public static void LogApplicationStarted(WebApplication app, ApiKeyOptions apiKeyOptions)
    {
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");

        logger.LogInformation(
            "Hotel Search API started | Environment: {Environment} | WriteAuth: {WriteAuthEnabled}",
            app.Environment.EnvironmentName,
            apiKeyOptions.IsEnabled);

        logger.LogInformation("Health endpoint: GET /health (includes PostgreSQL check)");

        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("Swagger UI: /swagger");
        }
    }
}
