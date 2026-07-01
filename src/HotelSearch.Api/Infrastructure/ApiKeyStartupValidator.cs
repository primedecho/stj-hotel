using HotelSearch.Api.Configuration;

namespace HotelSearch.Api.Infrastructure;

internal static class ApiKeyStartupValidator
{
    public static void EnsureProductionWriteKeyConfigured(IHostEnvironment environment, ApiKeyOptions options)
    {
        if (!environment.IsProduction())
        {
            return;
        }

        if (!options.IsEnabled)
        {
            throw new InvalidOperationException(
                "ApiKey:WriteKey must be configured when ASPNETCORE_ENVIRONMENT is Production.");
        }
    }
}
