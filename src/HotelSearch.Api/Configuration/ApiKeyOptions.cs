namespace HotelSearch.Api.Configuration;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";

    public const string HeaderName = "X-Api-Key";

    /// <summary>
    /// When set, POST/PUT/DELETE hotel endpoints require this value in the <see cref="HeaderName"/> header.
    /// When empty, write endpoints remain open (Development/Testing only).
    /// Required when <c>ASPNETCORE_ENVIRONMENT</c> is Production — startup fails if unset.
    /// </summary>
    public string? WriteKey { get; init; }

    public bool IsEnabled => !string.IsNullOrWhiteSpace(WriteKey);
}
