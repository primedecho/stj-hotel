using HotelSearch.Domain.Common;

namespace HotelSearch.Domain.Hotels;

/// <summary>
/// Hotel aggregate root.
/// </summary>
public sealed class Hotel
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Money Price { get; private set; } = null!;

    public GeoLocation Location { get; private set; } = null!;

    private Hotel()
    {
    }

    public Hotel(Guid id, string name, Money price, GeoLocation location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hotel name cannot be empty.");
        }

        Id = id;
        Name = name.Trim();
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }

    public void UpdateDetails(string name, Money price, GeoLocation location)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Hotel name cannot be empty.");
        }

        Name = name.Trim();
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Location = location ?? throw new ArgumentNullException(nameof(location));
    }
}
