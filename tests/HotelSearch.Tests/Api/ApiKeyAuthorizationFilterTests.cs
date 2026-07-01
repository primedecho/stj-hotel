using FluentAssertions;
using HotelSearch.Api.Configuration;
using HotelSearch.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HotelSearch.Tests.Api;

public class ApiKeyAuthorizationFilterTests
{
    private static readonly ApiKeyOptions EnabledOptions = new() { WriteKey = "secret-key" };

    [Fact]
    public async Task Allows_request_when_api_key_is_not_configured()
    {
        var filter = new ApiKeyAuthorizationFilter(Options.Create(new ApiKeyOptions()));
        var context = CreateContext(apiKey: null);
        var invoked = false;

        await filter.InvokeAsync(context, _ =>
        {
            invoked = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task Returns_401_when_api_key_header_is_missing()
    {
        var filter = new ApiKeyAuthorizationFilter(Options.Create(EnabledOptions));
        var context = CreateContext(apiKey: null);
        var invoked = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            invoked = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        invoked.Should().BeFalse();
        result.Should().NotBeNull();
        await ExecuteResultAsync(result!, context.HttpContext);
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Returns_401_when_api_key_is_invalid()
    {
        var filter = new ApiKeyAuthorizationFilter(Options.Create(EnabledOptions));
        var context = CreateContext(apiKey: "wrong-key");
        var invoked = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            invoked = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        invoked.Should().BeFalse();
        await ExecuteResultAsync(result!, context.HttpContext);
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Returns_401_when_api_key_length_differs_from_expected()
    {
        var filter = new ApiKeyAuthorizationFilter(Options.Create(EnabledOptions));
        var context = CreateContext(apiKey: "secret-key-extra");
        var invoked = false;

        var result = await filter.InvokeAsync(context, _ =>
        {
            invoked = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        invoked.Should().BeFalse();
        await ExecuteResultAsync(result!, context.HttpContext);
        context.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task Allows_request_when_api_key_is_valid()
    {
        var filter = new ApiKeyAuthorizationFilter(Options.Create(EnabledOptions));
        var context = CreateContext(apiKey: "secret-key");
        var invoked = false;

        await filter.InvokeAsync(context, _ =>
        {
            invoked = true;
            return ValueTask.FromResult<object?>(Results.Ok());
        });

        invoked.Should().BeTrue();
    }

    private static EndpointFilterInvocationContext CreateContext(string? apiKey)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton(Options.Create(EnabledOptions))
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext { RequestServices = services };

        if (apiKey is not null)
        {
            httpContext.Request.Headers[ApiKeyOptions.HeaderName] = apiKey;
        }

        return new DefaultEndpointFilterInvocationContext(httpContext);
    }

    private static async Task ExecuteResultAsync(object result, HttpContext httpContext)
    {
        if (result is IResult iResult)
        {
            await iResult.ExecuteAsync(httpContext);
        }
    }
}
