namespace HotelSearch.Application.Search;

public sealed record SearchHotelResult(
    Guid Id,
    string Name,
    decimal Price,
    double DistanceKm,
    double RankingScore);
