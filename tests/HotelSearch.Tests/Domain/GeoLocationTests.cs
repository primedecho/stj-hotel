using HotelSearch.Domain.Common;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Tests.Domain;

public class GeoLocationTests
{
    [Theory]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(90)]
    public void Creates_when_latitude_is_within_valid_range(double latitude)
    {
        var location = new GeoLocation(latitude, 0);

        Assert.Equal(latitude, location.Latitude);
    }

    [Theory]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(180)]
    public void Creates_when_longitude_is_within_valid_range(double longitude)
    {
        var location = new GeoLocation(0, longitude);

        Assert.Equal(longitude, location.Longitude);
    }

    [Theory]
    [InlineData(-90.1)]
    [InlineData(90.1)]
    public void Throws_when_latitude_is_out_of_range(double latitude)
    {
        var exception = Assert.Throws<DomainException>(() => new GeoLocation(latitude, 0));

        Assert.Equal("Latitude must be between -90 and 90.", exception.Message);
    }

    [Theory]
    [InlineData(-180.1)]
    [InlineData(180.1)]
    public void Throws_when_longitude_is_out_of_range(double longitude)
    {
        var exception = Assert.Throws<DomainException>(() => new GeoLocation(0, longitude));

        Assert.Equal("Longitude must be between -180 and 180.", exception.Message);
    }

    [Fact]
    public void DistanceToKilometers_returns_zero_for_same_location()
    {
        var location = new GeoLocation(45.815, 15.982);

        Assert.Equal(0, location.DistanceToKilometers(location), precision: 6);
    }

    [Fact]
    public void DistanceToKilometers_calculates_known_distance()
    {
        // Zagreb (~45.815, 15.982) to Split (~43.508, 16.440) — roughly 260 km by road;
        // great-circle distance is ~259 km.
        var zagreb = new GeoLocation(45.815, 15.982);
        var split = new GeoLocation(43.508, 16.440);

        var distance = zagreb.DistanceToKilometers(split);

        Assert.InRange(distance, 250, 270);
    }
}
