namespace HotelSearch.Tests.Api.Integration;

public sealed class IntegrationTestFixture : PostgresIntegrationFixtureBase
{
    public HttpClient Client { get; private set; } = null!;

    public IntegrationTestFixture()
        : base("hotels_integration")
    {
    }

    protected override void ConfigureAppSettings(Dictionary<string, string?> settings)
    {
    }

    protected override Task AfterInitializeAsync()
    {
        Client = Factory!.CreateClient();
        return Task.CompletedTask;
    }

    protected override Task BeforeDisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}
