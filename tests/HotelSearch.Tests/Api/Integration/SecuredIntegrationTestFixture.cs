namespace HotelSearch.Tests.Api.Integration;

/// <summary>
/// Integration fixture with API key authentication enabled for write endpoints.
/// </summary>
public sealed class SecuredIntegrationTestFixture : PostgresIntegrationFixtureBase
{
    public const string WriteApiKey = "integration-test-write-key";

    public HttpClient AnonymousClient { get; private set; } = null!;

    public HttpClient AuthenticatedClient { get; private set; } = null!;

    public SecuredIntegrationTestFixture()
        : base("hotels_secured")
    {
    }

    protected override void ConfigureAppSettings(Dictionary<string, string?> settings)
    {
        settings["ApiKey:WriteKey"] = WriteApiKey;
    }

    protected override Task AfterInitializeAsync()
    {
        AnonymousClient = Factory!.CreateClient();
        AuthenticatedClient = Factory.CreateClient();
        AuthenticatedClient.DefaultRequestHeaders.Add("X-Api-Key", WriteApiKey);
        return Task.CompletedTask;
    }

    protected override Task BeforeDisposeAsync()
    {
        AnonymousClient.Dispose();
        AuthenticatedClient.Dispose();
        return Task.CompletedTask;
    }
}

[CollectionDefinition(Name)]
public sealed class SecuredIntegrationTestCollection : ICollectionFixture<SecuredIntegrationTestFixture>
{
    public const string Name = "SecuredIntegration";
}
