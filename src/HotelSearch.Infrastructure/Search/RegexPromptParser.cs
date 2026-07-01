using System.Globalization;
using System.Text.RegularExpressions;
using HotelSearch.Application.Common;
using HotelSearch.Application.Search;
using HotelSearch.Domain.Common;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Infrastructure.Search;

/// <summary>
/// Parses search prompts in documented PoC formats.
/// Supported patterns:
/// <list type="bullet">
///   <item>"near 45.8150, 15.9819 under 200"</item>
///   <item>"location 45.8150, 15.9819 max price 150"</item>
///   <item>"from 45.8150, 15.9819 budget 300"</item>
///   <item>"hotels near 45.8150, 15.9819"</item>
/// </list>
/// </summary>
internal sealed partial class RegexPromptParser : IPromptParser
{
    public ParsedPrompt Parse(string prompt)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new AppException("Search prompt cannot be empty.");
        }

        var coordinates = TryExtractCoordinates(prompt)
            ?? throw new AppException(
                "Could not extract a location from the search prompt. " +
                "Use a supported format such as 'near 45.8150, 15.9819' or 'location 45.8150, 15.9819 max price 150'.");

        ValidateCoordinates(coordinates.Latitude, coordinates.Longitude);

        var maxBudget = TryExtractMaxBudget(prompt);
        if (maxBudget is < 0)
        {
            throw new AppException("Budget cannot be negative.");
        }

        return new ParsedPrompt(coordinates.Latitude, coordinates.Longitude, maxBudget);
    }

    private static (double Latitude, double Longitude)? TryExtractCoordinates(string prompt)
    {
        var prefixed = PrefixedCoordinatePattern().Match(prompt);
        if (TryParseCoordinateGroups(prefixed, out var coordinates))
        {
            return coordinates;
        }

        var pair = CoordinatePairPattern().Match(prompt);
        if (TryParseCoordinateGroups(pair, out coordinates))
        {
            return coordinates;
        }

        return null;
    }

    private static bool TryParseCoordinateGroups(Match match, out (double Latitude, double Longitude) coordinates)
    {
        coordinates = default;

        if (!match.Success)
        {
            return false;
        }

        if (!double.TryParse(match.Groups["lat"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            || !double.TryParse(match.Groups["lon"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
        {
            return false;
        }

        coordinates = (latitude, longitude);
        return true;
    }

    private static decimal? TryExtractMaxBudget(string prompt)
    {
        foreach (var pattern in new[] { UnderBudgetPattern(), MaxPricePattern(), BudgetPattern() })
        {
            var match = pattern.Match(prompt);
            if (!match.Success)
            {
                continue;
            }

            if (decimal.TryParse(match.Groups["value"].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var budget))
            {
                return budget;
            }
        }

        return null;
    }

    private static void ValidateCoordinates(double latitude, double longitude)
    {
        try
        {
            _ = new GeoLocation(latitude, longitude);
        }
        catch (DomainException ex)
        {
            throw new AppException(ex.Message);
        }
    }

    [GeneratedRegex(
        @"(?:\bnear|\blocation|\bfrom|\bhotels\s+near)\s+(?<lat>-?\d+(?:\.\d+)?)\s*,\s*(?<lon>-?\d+(?:\.\d+)?)",
        RegexOptions.IgnoreCase)]
    private static partial Regex PrefixedCoordinatePattern();

    [GeneratedRegex(@"(?<lat>-?\d+(?:\.\d+)?)\s*,\s*(?<lon>-?\d+(?:\.\d+)?)")]
    private static partial Regex CoordinatePairPattern();

    [GeneratedRegex(@"\bunder\s+(?<value>-?\d+(?:\.\d+)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex UnderBudgetPattern();

    [GeneratedRegex(@"\bmax\s+price\s+(?<value>-?\d+(?:\.\d+)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex MaxPricePattern();

    [GeneratedRegex(@"\bbudget\s+(?<value>-?\d+(?:\.\d+)?)\b", RegexOptions.IgnoreCase)]
    private static partial Regex BudgetPattern();
}
