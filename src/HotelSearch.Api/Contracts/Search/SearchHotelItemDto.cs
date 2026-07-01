namespace HotelSearch.Api.Contracts.Search;

public sealed record SearchHotelItemDto(
    Guid Id,
    string Name,
    decimal Price,
    double DistanceKm,
    double RankingScore);
