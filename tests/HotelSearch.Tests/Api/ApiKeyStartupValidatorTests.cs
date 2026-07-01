using FluentAssertions;
using HotelSearch.Api.Configuration;
using HotelSearch.Api.Infrastructure;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace HotelSearch.Tests.Api;

public class ApiKeyStartupValidatorTests
{
    [Fact]
    public void Does_not_throw_in_development_when_api_key_is_unset()
    {
        var act = () => ApiKeyStartupValidator.EnsureProductionWriteKeyConfigured(
            new TestHostEnvironment(Environments.Development),
            new ApiKeyOptions());

        act.Should().NotThrow();
    }

    [Fact]
    public void Throws_in_production_when_api_key_is_unset()
    {
        var act = () => ApiKeyStartupValidator.EnsureProductionWriteKeyConfigured(
            new TestHostEnvironment(Environments.Production),
            new ApiKeyOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*ApiKey:WriteKey*Production*");
    }

    [Fact]
    public void Does_not_throw_in_production_when_api_key_is_configured()
    {
        var act = () => ApiKeyStartupValidator.EnsureProductionWriteKeyConfigured(
            new TestHostEnvironment(Environments.Production),
            new ApiKeyOptions { WriteKey = "configured-key" });

        act.Should().NotThrow();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "HotelSearch.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
