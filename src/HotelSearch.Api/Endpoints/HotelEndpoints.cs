using HotelSearch.Api.Contracts.Common;
using HotelSearch.Api.Contracts.Hotels;
using HotelSearch.Api.Contracts.Search;
using HotelSearch.Api.Infrastructure;
using HotelSearch.Api.Mapping;
using HotelSearch.Application.Common;
using HotelSearch.Application.Hotels;
using HotelSearch.Application.Search;

namespace HotelSearch.Api.Endpoints;

internal static class HotelEndpoints
{
    public static RouteGroupBuilder MapHotelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/hotels")
            .WithTags("Hotels")
            .AddEndpointFilter<FluentValidationFilter>();

        group.MapPost("/", CreateHotel)
            .AddEndpointFilter<ApiKeyAuthorizationFilter>()
            .WithName("CreateHotel")
            .Produces<HotelDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/", ListHotels)
            .WithName("ListHotels")
            .Produces<IReadOnlyList<HotelDto>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetHotelById)
            .WithName("GetHotelById")
            .Produces<HotelDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateHotel)
            .AddEndpointFilter<ApiKeyAuthorizationFilter>()
            .WithName("UpdateHotel")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteHotel)
            .AddEndpointFilter<ApiKeyAuthorizationFilter>()
            .WithName("DeleteHotel")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/search", SearchHotels)
            .WithName("SearchHotels")
            .WithTags("Search")
            .Produces<PagedResponse<SearchHotelItemDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> CreateHotel(
        CreateHotelRequest request,
        IHotelService hotelService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(hotelService);

        var created = await hotelService.CreateAsync(request.ToCommand(), cancellationToken);
        return Results.Created($"/api/hotels/{created.Id}", created.ToDto());
    }

    private static async Task<IResult> ListHotels(
        IHotelService hotelService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(hotelService);

        var hotels = await hotelService.ListAsync(cancellationToken);
        return Results.Ok(hotels.Select(h => h.ToDto()));
    }

    private static async Task<IResult> GetHotelById(
        Guid id,
        IHotelService hotelService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(hotelService);

        var hotel = await hotelService.GetByIdAsync(id, cancellationToken);

        if (hotel is null)
        {
            throw new NotFoundException($"Hotel '{id}' was not found.");
        }

        return Results.Ok(hotel.ToDto());
    }

    private static async Task<IResult> UpdateHotel(
        Guid id,
        UpdateHotelRequest request,
        IHotelService hotelService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(hotelService);

        var updated = await hotelService.UpdateAsync(id, request.ToCommand(), cancellationToken);

        if (updated is null)
        {
            throw new NotFoundException($"Hotel '{id}' was not found.");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteHotel(
        Guid id,
        IHotelService hotelService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(hotelService);

        var deleted = await hotelService.DeleteAsync(id, cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException($"Hotel '{id}' was not found.");
        }

        return Results.NoContent();
    }

    private static async Task<IResult> SearchHotels(
        SearchHotelsRequestDto request,
        IHotelSearchService searchService,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(searchService);

        var result = await searchService.SearchAsync(request.ToApplicationRequest(), cancellationToken);
        return Results.Ok(result.ToPagedDto(r => r.ToDto()));
    }
}
