using HotelSearch.Application.Common;
using HotelSearch.Application.Hotels;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Application.Search;

public sealed class HotelSearchService(
    IHotelRepository repository,
    IPromptParser promptParser) : IHotelSearchService
{
    public const int DefaultPage = SearchConstants.DefaultPage;
    public const int DefaultPageSize = SearchConstants.DefaultPageSize;
    public const int MaxPageSize = SearchConstants.MaxPageSize;

    public async Task<PagedResult<SearchHotelResult>> SearchAsync(
        SearchHotelsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
        {
            throw new AppException("Search prompt cannot be empty.");
        }

        if (request.Page < 1)
        {
            throw new AppException("Page must be at least 1.");
        }

        if (request.PageSize < 1)
        {
            throw new AppException("Page size must be at least 1.");
        }

        if (request.PageSize > MaxPageSize)
        {
            throw new AppException($"Page size cannot exceed {MaxPageSize}.");
        }

        var parsed = promptParser.Parse(request.Prompt);
        var userLocation = new GeoLocation(parsed.Latitude, parsed.Longitude);

        var hotels = await repository.GetAllAsync(cancellationToken);

        var ranked = HotelSearchRanker.Rank(hotels, userLocation, parsed.MaxBudget);

        var totalCount = ranked.Count;
        var skip = (request.Page - 1) * request.PageSize;

        if (skip >= totalCount)
        {
            return new PagedResult<SearchHotelResult>([], request.Page, request.PageSize, totalCount);
        }

        var take = Math.Min(request.PageSize, totalCount - skip);
        var items = ranked.GetRange(skip, take);

        return new PagedResult<SearchHotelResult>(items, request.Page, request.PageSize, totalCount);
    }
}
