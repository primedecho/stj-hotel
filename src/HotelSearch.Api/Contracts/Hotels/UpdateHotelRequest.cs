namespace HotelSearch.Api.Contracts.Hotels;

public sealed class UpdateHotelRequest
{
    /// <example>Grand Hotel Zagreb</example>
    public string Name { get; init; } = string.Empty;

    /// <example>135.00</example>
    public decimal Price { get; init; }

    /// <example>45.8150</example>
    public double Latitude { get; init; }

    /// <example>15.9819</example>
    public double Longitude { get; init; }
}
