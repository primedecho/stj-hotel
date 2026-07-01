namespace HotelSearch.Api.Contracts.Search;

public sealed class SearchHotelsRequestDto
{
    /// <summary>
    /// Natural-language search prompt with location and optional budget.
    /// </summary>
    /// <example>near 45.8150, 15.9819 under 200</example>
    public string Prompt { get; init; } = string.Empty;

    /// <example>1</example>
    public int Page { get; init; } = 1;

    /// <example>10</example>
    public int PageSize { get; init; } = 10;
}
