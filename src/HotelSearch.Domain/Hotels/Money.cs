using HotelSearch.Domain.Common;

namespace HotelSearch.Domain.Hotels;

/// <summary>
/// Monetary amount for a hotel price. Ensures the value is positive.
/// </summary>
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; private set; }

    private Money()
    {
    }

    public Money(decimal amount)
    {
        if (amount <= 0)
        {
            throw new DomainException("Price must be greater than zero.");
        }

        Amount = amount;
    }

    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => Amount.GetHashCode();

    public override string ToString() => Amount.ToString("F2");
}
