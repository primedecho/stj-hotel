using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HotelSearch.Tests.Api.Integration;

/// <summary>
/// WebApplicationFactory that injects test configuration via <see cref="ConfigureWebHost"/>.
/// More reliable than inline <c>WithWebHostBuilder</c> configuration callbacks alone.
/// </summary>
internal sealed class IntegrationWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly IReadOnlyDictionary<string, string?> _settings;

    public IntegrationWebApplicationFactory(IReadOnlyDictionary<string, string?> settings)
    {
        _settings = settings;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        foreach (var (key, value) in _settings)
        {
            if (value is not null)
            {
                builder.UseSetting(key, value);
            }
        }

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(_settings);
        });
    }
}
