using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HotelSearch.Api.Infrastructure;

internal static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var databaseStatus = "unhealthy";
        if (report.Entries.TryGetValue("postgresql", out var databaseEntry))
        {
            databaseStatus = databaseEntry.Status switch
            {
                HealthStatus.Healthy => "healthy",
                HealthStatus.Degraded => "degraded",
                _ => "unhealthy"
            };
        }

        var overallStatus = report.Status switch
        {
            HealthStatus.Healthy => "healthy",
            HealthStatus.Degraded => "degraded",
            _ => "unhealthy"
        };

        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(
            new HealthResponse(overallStatus, databaseStatus),
            JsonOptions);
    }

    private sealed record HealthResponse(string Status, string Database);
}
