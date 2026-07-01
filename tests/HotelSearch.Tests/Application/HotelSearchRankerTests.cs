using HotelSearch.Application.Search;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Tests.Application;

public class HotelSearchRankerTests
{
    [Fact]
    public void CalculateScore_prefers_cheaper_and_closer_hotels()
    {
        var cheaperCloser = HotelSearchRanker.CalculateScore(
            50m, 5, minPrice: 50, maxPrice: 200, minDistance: 5, maxDistance: 50, maxBudget: null);

        var expensiveFarther = HotelSearchRanker.CalculateScore(
            200m, 50, minPrice: 50, maxPrice: 200, minDistance: 5, maxDistance: 50, maxBudget: null);

        Assert.True(cheaperCloser < expensiveFarther);
    }

    [Fact]
    public void CalculateScore_returns_zero_price_component_when_all_prices_are_equal()
    {
        var score = HotelSearchRanker.CalculateScore(
            100m, 10, minPrice: 100, maxPrice: 100, minDistance: 5, maxDistance: 20, maxBudget: null);

        Assert.Equal(HotelSearchRanker.DistanceWeight * (5.0 / 15.0), score, precision: 6);
    }

    [Fact]
    public void CalculateScore_returns_zero_distance_component_when_all_distances_are_equal()
    {
        var score = HotelSearchRanker.CalculateScore(
            100m, 10, minPrice: 50, maxPrice: 150, minDistance: 10, maxDistance: 10, maxBudget: null);

        Assert.Equal(HotelSearchRanker.PriceWeight * 0.5, score, precision: 6);
    }

    [Fact]
    public void CalculateScore_penalizes_hotels_over_budget()
    {
        var withinBudget = HotelSearchRanker.CalculateScore(
            100m, 10, minPrice: 100, maxPrice: 100, minDistance: 10, maxDistance: 10, maxBudget: 150m);

        var overBudget = HotelSearchRanker.CalculateScore(
            100m, 10, minPrice: 100, maxPrice: 100, minDistance: 10, maxDistance: 10, maxBudget: 50m);

        Assert.True(withinBudget < overBudget);
        Assert.Equal(HotelSearchRanker.OverBudgetPenalty, overBudget - withinBudget, precision: 6);
    }

    [Fact]
    public void Rank_orders_by_name_when_price_and_distance_match()
    {
        var userLocation = new GeoLocation(45.0, 15.0);
        var hotels = new[]
        {
            new Hotel(Guid.NewGuid(), "Zulu", new Money(100m), new GeoLocation(45.01, 15.01)),
            new Hotel(Guid.NewGuid(), "Alpha", new Money(100m), new GeoLocation(45.01, 15.01)),
        };

        var ranked = HotelSearchRanker.Rank(hotels, userLocation, maxBudget: null);

        Assert.Equal("Alpha", ranked[0].Name);
        Assert.Equal("Zulu", ranked[1].Name);
        Assert.Equal(ranked[0].RankingScore, ranked[1].RankingScore);
    }

    [Fact]
    public void Normalize_returns_zero_when_min_equals_max()
    {
        Assert.Equal(0, HotelSearchRanker.Normalize(42, 42, 42));
    }
}
