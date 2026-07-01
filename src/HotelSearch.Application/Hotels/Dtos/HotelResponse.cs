namespace HotelSearch.Application.Hotels.Dtos;

public sealed record HotelResponse(
    Guid Id,
    string Name,
    decimal Price,
    double Latitude,
    double Longitude);
