namespace HotelSearch.Application.Search;

public sealed record ParsedPrompt(
    double Latitude,
    double Longitude,
    decimal? MaxBudget);
