namespace HotelSearch.Api.Contracts.Hotels;

public sealed class CreateHotelRequest
{
    /// <summary>Hotel display name.</summary>
    /// <example>Grand Hotel Zagreb</example>
    public string Name { get; init; } = string.Empty;

    /// <summary>Nightly price in the default currency.</summary>
    /// <example>120.00</example>
    public decimal Price { get; init; }

    /// <summary>Latitude in decimal degrees.</summary>
    /// <example>45.8150</example>
    public double Latitude { get; init; }

    /// <summary>Longitude in decimal degrees.</summary>
    /// <example>15.9819</example>
    public double Longitude { get; init; }
}
