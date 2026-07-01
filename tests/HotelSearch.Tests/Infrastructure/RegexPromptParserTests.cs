using HotelSearch.Application.Common;
using HotelSearch.Application.Search;
using HotelSearch.Infrastructure.Search;

namespace HotelSearch.Tests.Infrastructure;

public class RegexPromptParserTests
{
    private readonly IPromptParser _parser = new RegexPromptParser();

    [Fact]
    public void Parse_extracts_location_only_when_budget_is_missing()
    {
        var result = _parser.Parse("hotels near 45.8150, 15.9819");

        Assert.Equal(45.8150, result.Latitude, precision: 4);
        Assert.Equal(15.9819, result.Longitude, precision: 4);
        Assert.Null(result.MaxBudget);
    }

    [Theory]
    [InlineData("near 45.8150, 15.9819 under 200", 200)]
    [InlineData("location 45.8150, 15.9819 max price 150", 150)]
    [InlineData("from 45.8150, 15.9819 budget 300", 300)]
    public void Parse_extracts_location_and_budget_from_supported_formats(string prompt, decimal expectedBudget)
    {
        var result = _parser.Parse(prompt);

        Assert.Equal(45.8150, result.Latitude, precision: 4);
        Assert.Equal(15.9819, result.Longitude, precision: 4);
        Assert.Equal(expectedBudget, result.MaxBudget);
    }

    [Theory]
    [InlineData("near 91.0, 15.9819", "Latitude")]
    [InlineData("location 45.8150, 181.0", "Longitude")]
    [InlineData("from -90.1, 15.9819", "Latitude")]
    public void Parse_throws_when_coordinates_are_invalid(string prompt, string expectedInMessage)
    {
        var exception = Assert.Throws<AppException>(() => _parser.Parse(prompt));

        Assert.Contains(expectedInMessage, exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("hotels under 200")]
    [InlineData("find something cheap")]
    [InlineData("max price 150")]
    public void Parse_throws_when_location_is_missing(string prompt)
    {
        var exception = Assert.Throws<AppException>(() => _parser.Parse(prompt));

        Assert.Contains("Could not extract a location", exception.Message);
    }

    [Theory]
    [InlineData("near 45.8150, 15.9819 under -50")]
    [InlineData("location 45.8150, 15.9819 max price -1")]
    [InlineData("from 45.8150, 15.9819 budget -0.01")]
    public void Parse_throws_when_budget_is_negative(string prompt)
    {
        var exception = Assert.Throws<AppException>(() => _parser.Parse(prompt));

        Assert.Equal("Budget cannot be negative.", exception.Message);
    }

    [Theory]
    [InlineData("NEAR 45.8150, 15.9819 UNDER 200")]
    [InlineData("Hotels Near 45.8150, 15.9819")]
    [InlineData("LOCATION 45.8150, 15.9819 MAX PRICE 150")]
    public void Parse_is_case_insensitive(string prompt)
    {
        var result = _parser.Parse(prompt);

        Assert.Equal(45.8150, result.Latitude, precision: 4);
        Assert.Equal(15.9819, result.Longitude, precision: 4);
    }
}
