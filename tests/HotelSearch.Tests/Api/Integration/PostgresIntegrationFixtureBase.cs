using HotelSearch.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace HotelSearch.Tests.Api.Integration;

public abstract class PostgresIntegrationFixtureBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    protected WebApplicationFactory<Program>? Factory { get; private set; }

    protected PostgresIntegrationFixtureBase(string databaseName)
    {
        _postgres = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase(databaseName)
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    protected abstract void ConfigureAppSettings(Dictionary<string, string?> settings);

    public virtual async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString()
        };

        ConfigureAppSettings(settings);

        Factory = new IntegrationWebApplicationFactory(settings);

        await DatabaseInitializer.ApplyMigrationsAsync(Factory.Services);
        await AfterInitializeAsync();
    }

    protected virtual Task AfterInitializeAsync() => Task.CompletedTask;

    public async Task ResetDatabaseAsync()
    {
        if (Factory is null)
        {
            return;
        }

        await using var scope = Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HotelSearchDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE hotels;");
    }

    public virtual async Task DisposeAsync()
    {
        await BeforeDisposeAsync();

        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    protected virtual Task BeforeDisposeAsync() => Task.CompletedTask;
}
