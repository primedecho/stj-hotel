using System.Security.Cryptography;
using System.Text;
using HotelSearch.Api.Configuration;
using Microsoft.Extensions.Options;

namespace HotelSearch.Api.Infrastructure;

internal sealed class ApiKeyAuthorizationFilter(IOptions<ApiKeyOptions> options) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var apiKeyOptions = options.Value;

        if (!apiKeyOptions.IsEnabled)
        {
            return next(context);
        }

        if (!TryValidateApiKey(context.HttpContext, apiKeyOptions.WriteKey!))
        {
            var problem = ApiProblemDetails.CreateProblem(
                context.HttpContext,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                "A valid API key is required for this operation.",
                ApiProblemDetails.UnauthorizedType);

            return ValueTask.FromResult<object?>(
                Results.Json(problem, statusCode: StatusCodes.Status401Unauthorized));
        }

        return next(context);
    }

    private static bool TryValidateApiKey(HttpContext httpContext, string expectedKey)
    {
        if (!httpContext.Request.Headers.TryGetValue(ApiKeyOptions.HeaderName, out var providedValues))
        {
            return false;
        }

        var providedKey = providedValues.ToString();

        if (string.IsNullOrEmpty(providedKey))
        {
            return false;
        }

        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expectedKey));
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(providedKey));

        return CryptographicOperations.FixedTimeEquals(expectedHash, providedHash);
    }
}
