namespace HotelSearch.Api.Contracts.Hotels;

public sealed record HotelDto(
    Guid Id,
    string Name,
    decimal Price,
    double Latitude,
    double Longitude);
