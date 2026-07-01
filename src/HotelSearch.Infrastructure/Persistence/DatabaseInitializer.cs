using HotelSearch.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelSearch.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    private const int DefaultMaxAttempts = 10;
    private static readonly TimeSpan InitialRetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Applies pending EF Core migrations with retries until PostgreSQL is reachable.
    /// </summary>
    public static async Task ApplyMigrationsAsync(
        IServiceProvider services,
        CancellationToken cancellationToken = default,
        int maxAttempts = DefaultMaxAttempts)
    {
        var logger = services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DatabaseInitializer));

        var delay = InitialRetryDelay;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var context = scope.ServiceProvider.GetRequiredService<HotelSearchDbContext>();

                await context.Database.MigrateAsync(cancellationToken);

                logger.LogInformation("Database migrations applied successfully.");

                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsTransientDatabaseError(ex))
            {
                logger.LogWarning(
                    "Database not ready (attempt {Attempt}/{MaxAttempts}): {Message}. Retrying in {DelaySeconds}s...",
                    attempt,
                    maxAttempts,
                    ex.Message,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 10));
            }
        }
    }

    private static bool IsTransientDatabaseError(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TimeoutException or System.Net.Sockets.SocketException or IOException)
            {
                return true;
            }

            var typeName = current.GetType().FullName ?? string.Empty;
            if (typeName.Contains("Npgsql", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
