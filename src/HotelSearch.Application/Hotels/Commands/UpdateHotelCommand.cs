namespace HotelSearch.Application.Hotels.Commands;

public sealed record UpdateHotelCommand(
    string Name,
    decimal Price,
    double Latitude,
    double Longitude);
