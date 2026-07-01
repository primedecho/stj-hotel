using HotelSearch.Application.Hotels;
using HotelSearch.Application.Search;
using HotelSearch.Infrastructure.Persistence;
using HotelSearch.Infrastructure.Persistence.Repositories;
using HotelSearch.Infrastructure.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HotelSearch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<HotelSearchDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        services.AddHealthChecks()
            .AddDbContextCheck<HotelSearchDbContext>(
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["db", "ready"]);

        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IPromptParser, RegexPromptParser>();

        return services;
    }
}
