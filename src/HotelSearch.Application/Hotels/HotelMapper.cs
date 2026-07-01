using HotelSearch.Application.Hotels.Dtos;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Application.Hotels;

internal static class HotelMapper
{
    public static HotelResponse ToResponse(Hotel hotel) =>
        new(
            hotel.Id,
            hotel.Name,
            hotel.Price.Amount,
            hotel.Location.Latitude,
            hotel.Location.Longitude);
}
