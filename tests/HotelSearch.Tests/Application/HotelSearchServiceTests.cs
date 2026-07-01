using HotelSearch.Application.Common;
using HotelSearch.Application.Hotels;
using HotelSearch.Application.Search;
using HotelSearch.Domain.Hotels;
using Moq;

namespace HotelSearch.Tests.Application;

public class HotelSearchServiceTests
{
    private readonly Mock<IHotelRepository> _repository = new();
    private readonly Mock<IPromptParser> _promptParser = new();
    private readonly HotelSearchService _sut;

    public HotelSearchServiceTests()
    {
        _sut = new HotelSearchService(_repository.Object, _promptParser.Object);
    }

    [Fact]
    public async Task SearchAsync_returns_name_price_and_distance()
    {
        var userLocation = new ParsedPrompt(45.0, 15.0, null);
        var hotel = CreateHotel("Grand Hotel", 120m, 45.01, 15.01);

        _promptParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(userLocation);
        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([hotel]);

        var result = await _sut.SearchAsync(new SearchHotelsRequest("hotels near me"));

        var item = Assert.Single(result.Items);
        Assert.Equal("Grand Hotel", item.Name);
        Assert.Equal(120m, item.Price);
        Assert.True(item.DistanceKm > 0);
    }

    [Fact]
    public async Task SearchAsync_orders_cheaper_and_closer_hotels_first()
    {
        _promptParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.0, 15.0, null));

        var cheapClose = CreateHotel("Budget Inn", 50m, 45.001, 15.001);
        var expensiveFar = CreateHotel("Luxury Resort", 300m, 46.0, 16.0);

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([expensiveFar, cheapClose]);

        var result = await _sut.SearchAsync(new SearchHotelsRequest("find hotels"));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Budget Inn", result.Items[0].Name);
        Assert.Equal("Luxury Resort", result.Items[1].Name);
        Assert.True(result.Items[0].RankingScore < result.Items[1].RankingScore);
    }

    [Fact]
    public async Task SearchAsync_ranks_expensive_and_far_hotels_lower()
    {
        _promptParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.0, 15.0, null));

        var hotels = new[]
        {
            CreateHotel("Mid Range", 100m, 45.05, 15.05),
            CreateHotel("Premium Far", 250m, 46.5, 17.0),
            CreateHotel("Economy Near", 60m, 45.001, 15.001),
        };

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var result = await _sut.SearchAsync(new SearchHotelsRequest("hotels"));

        Assert.Equal("Economy Near", result.Items[0].Name);
        Assert.Equal("Premium Far", result.Items[^1].Name);
    }

    [Fact]
    public async Task SearchAsync_uses_deterministic_ordering_when_scores_are_equal()
    {
        _promptParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.0, 15.0, null));

        var hotels = new[]
        {
            CreateHotel("Zulu Hotel", 100m, 45.01, 15.01),
            CreateHotel("Alpha Hotel", 100m, 45.01, 15.01),
        };

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var first = await _sut.SearchAsync(new SearchHotelsRequest("hotels"));
        var second = await _sut.SearchAsync(new SearchHotelsRequest("hotels"));

        Assert.Equal(first.Items.Select(i => i.Name), second.Items.Select(i => i.Name));
        Assert.Equal("Alpha Hotel", first.Items[0].Name);
        Assert.Equal("Zulu Hotel", first.Items[1].Name);
    }

    [Fact]
    public async Task SearchAsync_prefers_hotels_within_budget()
    {
        _promptParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.0, 15.0, MaxBudget: 100m));

        var withinBudget = CreateHotel("Affordable Stay", 80m, 45.2, 15.2);
        var overBudget = CreateHotel("Premium Stay", 150m, 45.001, 15.001);

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([overBudget, withinBudget]);

        var result = await _sut.SearchAsync(new SearchHotelsRequest("under 100"));

        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Affordable Stay", result.Items[0].Name);
        Assert.Equal("Premium Stay", result.Items[1].Name);
        Assert.True(result.Items[0].RankingScore < result.Items[1].RankingScore);
    }

    [Fact]
    public async Task SearchAsync_applies_paging_metadata()
    {
        _promptParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.0, 15.0, null));

        var hotels = Enumerable.Range(1, 5)
            .Select(i => CreateHotel($"Hotel {i}", 50m + i, 45.0 + i * 0.001, 15.0))
            .ToList();

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(hotels);

        var result = await _sut.SearchAsync(new SearchHotelsRequest("hotels", Page: 2, PageSize: 2));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task SearchAsync_uses_default_paging_when_not_specified()
    {
        _promptParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.0, 15.0, null));

        _repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([CreateHotel("Only Hotel", 100m, 45.01, 15.01)]);

        var result = await _sut.SearchAsync(new SearchHotelsRequest("hotels"));

        Assert.Equal(HotelSearchService.DefaultPage, result.Page);
        Assert.Equal(HotelSearchService.DefaultPageSize, result.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchAsync_throws_when_page_is_invalid(int page)
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.SearchAsync(new SearchHotelsRequest("hotels", Page: page)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task SearchAsync_throws_when_page_size_is_invalid(int pageSize)
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.SearchAsync(new SearchHotelsRequest("hotels", PageSize: pageSize)));
    }

    [Fact]
    public async Task SearchAsync_throws_when_page_size_exceeds_maximum()
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.SearchAsync(new SearchHotelsRequest("hotels", PageSize: 101)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchAsync_throws_when_prompt_is_empty(string? prompt)
    {
        await Assert.ThrowsAsync<AppException>(() =>
            _sut.SearchAsync(new SearchHotelsRequest(prompt!)));
    }

    private static Hotel CreateHotel(string name, decimal price, double latitude, double longitude) =>
        new(Guid.NewGuid(), name, new Money(price), new GeoLocation(latitude, longitude));
}
