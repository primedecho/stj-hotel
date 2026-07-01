using HotelSearch.Domain.Hotels;

namespace HotelSearch.Application.Search;

/// <summary>
/// Computes normalized search ranking scores for hotel results.
/// </summary>
internal static class HotelSearchRanker
{
    public const double PriceWeight = 0.5;
    public const double DistanceWeight = 0.5;

    /// <summary>
    /// Penalty added to the score when a hotel exceeds the optional budget.
    /// Ensures in-budget hotels rank above out-of-budget hotels.
    /// </summary>
    public const double OverBudgetPenalty = 1.0;

    internal static List<SearchHotelResult> Rank(
        IReadOnlyList<Hotel> hotels,
        GeoLocation userLocation,
        decimal? maxBudget)
    {
        var count = hotels.Count;
        if (count == 0)
        {
            return [];
        }

        var candidates = new List<HotelCandidate>(count);
        var minPrice = double.MaxValue;
        var maxPrice = double.MinValue;
        var minDistance = double.MaxValue;
        var maxDistance = double.MinValue;

        foreach (var hotel in hotels)
        {
            var price = (double)hotel.Price.Amount;
            var distanceKm = userLocation.DistanceToKilometers(hotel.Location);

            candidates.Add(new HotelCandidate(hotel.Id, hotel.Name, hotel.Price.Amount, distanceKm));

            if (price < minPrice)
            {
                minPrice = price;
            }

            if (price > maxPrice)
            {
                maxPrice = price;
            }

            if (distanceKm < minDistance)
            {
                minDistance = distanceKm;
            }

            if (distanceKm > maxDistance)
            {
                maxDistance = distanceKm;
            }
        }

        var results = new List<SearchHotelResult>(count);
        foreach (var candidate in candidates)
        {
            results.Add(new SearchHotelResult(
                candidate.Id,
                candidate.Name,
                candidate.Price,
                candidate.DistanceKm,
                CalculateScore(
                    candidate.Price,
                    candidate.DistanceKm,
                    minPrice,
                    maxPrice,
                    minDistance,
                    maxDistance,
                    maxBudget)));
        }

        results.Sort(SearchHotelResultComparer.Instance);
        return results;
    }

    public static double CalculateScore(
        decimal price,
        double distanceKm,
        double minPrice,
        double maxPrice,
        double minDistance,
        double maxDistance,
        decimal? maxBudget)
    {
        var normalizedPrice = Normalize((double)price, minPrice, maxPrice);
        var normalizedDistance = Normalize(distanceKm, minDistance, maxDistance);

        var score = (PriceWeight * normalizedPrice) + (DistanceWeight * normalizedDistance);

        if (maxBudget.HasValue && price > maxBudget.Value)
        {
            score += OverBudgetPenalty;
        }

        return score;
    }

    /// <summary>
    /// Min-max normalization to [0, 1]. Returns 0 when all values in the range are equal.
    /// </summary>
    internal static double Normalize(double value, double min, double max) =>
        max <= min ? 0 : (value - min) / (max - min);

    private sealed record HotelCandidate(Guid Id, string Name, decimal Price, double DistanceKm);

    private sealed class SearchHotelResultComparer : IComparer<SearchHotelResult>
    {
        public static SearchHotelResultComparer Instance { get; } = new();

        public int Compare(SearchHotelResult? x, SearchHotelResult? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            var scoreComparison = x.RankingScore.CompareTo(y.RankingScore);
            if (scoreComparison != 0)
            {
                return scoreComparison;
            }

            var priceComparison = x.Price.CompareTo(y.Price);
            if (priceComparison != 0)
            {
                return priceComparison;
            }

            var distanceComparison = x.DistanceKm.CompareTo(y.DistanceKm);
            if (distanceComparison != 0)
            {
                return distanceComparison;
            }

            return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
        }
    }
}
