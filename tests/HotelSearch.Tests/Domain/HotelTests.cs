using HotelSearch.Domain.Common;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Tests.Domain;

public class HotelTests
{
    [Fact]
    public void Creates_with_valid_properties()
    {
        var id = Guid.NewGuid();
        var price = new Money(120m);
        var location = new GeoLocation(45.815, 15.982);

        var hotel = new Hotel(id, "Grand Hotel", price, location);

        Assert.Equal(id, hotel.Id);
        Assert.Equal("Grand Hotel", hotel.Name);
        Assert.Equal(120m, hotel.Price.Amount);
        Assert.Equal(45.815, hotel.Location.Latitude);
        Assert.Equal(15.982, hotel.Location.Longitude);
    }

    [Fact]
    public void Trims_hotel_name()
    {
        var hotel = new Hotel(
            Guid.NewGuid(),
            "  Grand Hotel  ",
            new Money(120m),
            new GeoLocation(45.815, 15.982));

        Assert.Equal("Grand Hotel", hotel.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Throws_when_name_is_empty(string? name)
    {
        var exception = Assert.Throws<DomainException>(() =>
            new Hotel(Guid.NewGuid(), name!, new Money(120m), new GeoLocation(45.815, 15.982)));

        Assert.Equal("Hotel name cannot be empty.", exception.Message);
    }

    [Fact]
    public void Throws_when_price_is_not_positive()
    {
        Assert.Throws<DomainException>(() =>
            new Hotel(Guid.NewGuid(), "Grand Hotel", new Money(0), new GeoLocation(45.815, 15.982)));
    }

    [Fact]
    public void Throws_when_location_is_invalid()
    {
        Assert.Throws<DomainException>(() =>
            new Hotel(Guid.NewGuid(), "Grand Hotel", new Money(120m), new GeoLocation(91, 0)));
    }

    [Fact]
    public void UpdateDetails_changes_name_price_and_location()
    {
        var hotel = new Hotel(
            Guid.NewGuid(),
            "Original",
            new Money(100m),
            new GeoLocation(45.0, 15.0));

        hotel.UpdateDetails(
            "Updated",
            new Money(150m),
            new GeoLocation(46.0, 16.0));

        Assert.Equal("Updated", hotel.Name);
        Assert.Equal(150m, hotel.Price.Amount);
        Assert.Equal(46.0, hotel.Location.Latitude);
        Assert.Equal(16.0, hotel.Location.Longitude);
    }

    [Fact]
    public void UpdateDetails_trims_name()
    {
        var hotel = new Hotel(
            Guid.NewGuid(),
            "Original",
            new Money(100m),
            new GeoLocation(45.0, 15.0));

        hotel.UpdateDetails(
            "  Trimmed  ",
            new Money(100m),
            new GeoLocation(45.0, 15.0));

        Assert.Equal("Trimmed", hotel.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_throws_when_name_is_empty(string? name)
    {
        var hotel = new Hotel(
            Guid.NewGuid(),
            "Original",
            new Money(100m),
            new GeoLocation(45.0, 15.0));

        var exception = Assert.Throws<DomainException>(() =>
            hotel.UpdateDetails(name!, new Money(100m), new GeoLocation(45.0, 15.0)));

        Assert.Equal("Hotel name cannot be empty.", exception.Message);
    }
}
