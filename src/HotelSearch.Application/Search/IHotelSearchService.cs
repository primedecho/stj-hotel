using HotelSearch.Application.Common;

namespace HotelSearch.Application.Search;

public interface IHotelSearchService
{
    Task<PagedResult<SearchHotelResult>> SearchAsync(
        SearchHotelsRequest request,
        CancellationToken cancellationToken = default);
}
