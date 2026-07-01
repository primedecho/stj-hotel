namespace HotelSearch.Application.Hotels.Commands;

public sealed record CreateHotelCommand(
    string Name,
    decimal Price,
    double Latitude,
    double Longitude);
