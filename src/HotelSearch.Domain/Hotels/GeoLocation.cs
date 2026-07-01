using HotelSearch.Domain.Common;

namespace HotelSearch.Domain.Hotels;

/// <summary>
/// Geographic coordinates. Supports distance calculation via the Haversine formula.
/// </summary>
public sealed class GeoLocation : IEquatable<GeoLocation>
{
    private const double EarthRadiusKm = 6371.0;

    public double Latitude { get; private set; }

    public double Longitude { get; private set; }

    private GeoLocation()
    {
    }

    public GeoLocation(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
        {
            throw new DomainException("Latitude must be between -90 and 90.");
        }

        if (longitude is < -180 or > 180)
        {
            throw new DomainException("Longitude must be between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Returns the great-circle distance to another location in kilometres.
    /// </summary>
    public double DistanceToKilometers(GeoLocation other)
    {
        ArgumentNullException.ThrowIfNull(other);

        var lat1 = ToRadians(Latitude);
        var lat2 = ToRadians(other.Latitude);
        var deltaLat = ToRadians(other.Latitude - Latitude);
        var deltaLon = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                + Math.Cos(lat1) * Math.Cos(lat2)
                * Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    public bool Equals(GeoLocation? other) =>
        other is not null
        && Latitude.Equals(other.Latitude)
        && Longitude.Equals(other.Longitude);

    public override bool Equals(object? obj) => Equals(obj as GeoLocation);

    public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
