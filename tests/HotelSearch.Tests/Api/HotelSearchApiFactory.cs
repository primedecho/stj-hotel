using HotelSearch.Application.Hotels;
using HotelSearch.Application.Search;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace HotelSearch.Tests.Api;

public sealed class HotelSearchApiFactory : WebApplicationFactory<Program>
{
    public Mock<IHotelRepository> RepositoryMock { get; } = new();

    public Mock<IPromptParser> PromptParserMock { get; } = new();

    public void ResetMocks()
    {
        RepositoryMock.Reset();
        PromptParserMock.Reset();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        const string connectionString =
            "Host=localhost;Port=5432;Database=hotels_test;Username=postgres;Password=postgres";

        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            ReplaceService(services, RepositoryMock.Object);
            ReplaceService(services, PromptParserMock.Object);
        });
    }

    private static void ReplaceService<T>(IServiceCollection services, T implementation)
        where T : class
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor is not null)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton(implementation);
    }
}
