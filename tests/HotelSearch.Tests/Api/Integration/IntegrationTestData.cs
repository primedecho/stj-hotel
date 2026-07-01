using HotelSearch.Api.Contracts.Hotels;

namespace HotelSearch.Tests.Api.Integration;

public static class IntegrationTestData
{
    public static CreateHotelRequest SampleHotel(string name, decimal price, double latitude, double longitude) =>
        new()
        {
            Name = name,
            Price = price,
            Latitude = latitude,
            Longitude = longitude
        };
}
