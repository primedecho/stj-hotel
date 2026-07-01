using HotelSearch.Api.Contracts.Common;
using HotelSearch.Api.Contracts.Hotels;
using HotelSearch.Api.Contracts.Search;
using HotelSearch.Application.Common;
using HotelSearch.Application.Hotels.Commands;
using HotelSearch.Application.Hotels.Dtos;
using HotelSearch.Application.Search;

namespace HotelSearch.Api.Mapping;

internal static class ApiMappingExtensions
{
    public static CreateHotelCommand ToCommand(this CreateHotelRequest request) =>
        new(request.Name, request.Price, request.Latitude, request.Longitude);

    public static UpdateHotelCommand ToCommand(this UpdateHotelRequest request) =>
        new(request.Name, request.Price, request.Latitude, request.Longitude);

    public static SearchHotelsRequest ToApplicationRequest(this SearchHotelsRequestDto request) =>
        new(request.Prompt, request.Page, request.PageSize);

    public static HotelDto ToDto(this HotelResponse response) =>
        new(response.Id, response.Name, response.Price, response.Latitude, response.Longitude);

    public static SearchHotelItemDto ToDto(this SearchHotelResult result) =>
        new(result.Id, result.Name, result.Price, result.DistanceKm, result.RankingScore);

    public static PagedResponse<TDto> ToPagedDto<TItem, TDto>(
        this PagedResult<TItem> paged,
        Func<TItem, TDto> mapper) =>
        new(
            paged.Items.Select(mapper).ToList(),
            paged.Page,
            paged.PageSize,
            paged.TotalCount,
            paged.TotalPages);
}
