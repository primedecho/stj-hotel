using HotelSearch.Domain.Common;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Tests.Domain;

public class MoneyTests
{
    [Fact]
    public void Creates_when_amount_is_positive()
    {
        var money = new Money(99.99m);

        Assert.Equal(99.99m, money.Amount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-0.01)]
    public void Throws_when_amount_is_not_positive(decimal amount)
    {
        var exception = Assert.Throws<DomainException>(() => new Money(amount));

        Assert.Equal("Price must be greater than zero.", exception.Message);
    }

    [Fact]
    public void Value_objects_with_same_amount_are_equal()
    {
        var left = new Money(50m);
        var right = new Money(50m);

        Assert.True(left.Equals(right));
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }
}
