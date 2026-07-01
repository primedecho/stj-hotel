using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HotelSearch.Api.Contracts.Hotels;
using HotelSearch.Api.Contracts.Search;
using HotelSearch.Api.Validation;
using HotelSearch.Application.Common;
using HotelSearch.Application.Hotels;
using HotelSearch.Application.Search;
using HotelSearch.Domain.Hotels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HotelSearch.Tests.Api;

public class HotelApiValidationTests : IClassFixture<HotelSearchApiFactory>
{
    private readonly HttpClient _client;
    private readonly HotelSearchApiFactory _factory;

    public HotelApiValidationTests(HotelSearchApiFactory factory)
    {
        _factory = factory;
        _factory.ResetMocks();
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateHotel_returns_400_when_name_is_empty()
    {
        var response = await PostHotelAsync(new CreateHotelRequest
        {
            Name = "   ",
            Price = 100,
            Latitude = 45,
            Longitude = 15
        });

        await AssertValidationProblemAsync(response, "name", "Hotel name cannot be empty.");
    }

    [Fact]
    public async Task CreateHotel_returns_400_when_price_is_zero_or_negative()
    {
        var zeroResponse = await PostHotelAsync(new CreateHotelRequest
        {
            Name = "Hotel",
            Price = 0,
            Latitude = 45,
            Longitude = 15
        });

        await AssertValidationProblemAsync(zeroResponse, "price", "Price must be greater than zero.");

        var negativeResponse = await PostHotelAsync(new CreateHotelRequest
        {
            Name = "Hotel",
            Price = -10,
            Latitude = 45,
            Longitude = 15
        });

        await AssertValidationProblemAsync(negativeResponse, "price", "Price must be greater than zero.");
    }

    [Fact]
    public async Task CreateHotel_returns_400_when_coordinates_are_invalid()
    {
        var response = await PostHotelAsync(new CreateHotelRequest
        {
            Name = "Hotel",
            Price = 100,
            Latitude = 91,
            Longitude = 15
        });

        await AssertValidationProblemAsync(response, "latitude", "Latitude must be between -90 and 90.");
    }

    [Fact]
    public async Task UpdateHotel_returns_404_when_hotel_does_not_exist()
    {
        var id = Guid.NewGuid();
        _factory.RepositoryMock
            .Setup(r => r.GetByIdForUpdateAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Hotel?)null);

        var response = await _client.PutAsJsonAsync($"/api/hotels/{id}", new UpdateHotelRequest
        {
            Name = "Updated",
            Price = 100,
            Latitude = 45,
            Longitude = 15
        });

        await AssertNotFoundAsync(response, $"Hotel '{id}' was not found.");
    }

    [Fact]
    public async Task DeleteHotel_returns_404_when_hotel_does_not_exist()
    {
        var id = Guid.NewGuid();
        _factory.RepositoryMock
            .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var response = await _client.DeleteAsync($"/api/hotels/{id}");

        await AssertNotFoundAsync(response, $"Hotel '{id}' was not found.");
    }

    [Fact]
    public async Task Search_returns_400_when_prompt_has_no_location()
    {
        _factory.PromptParserMock
            .Setup(p => p.Parse(It.IsAny<string>()))
            .Throws(new AppException("Could not extract a location from the search prompt."));

        var response = await PostSearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "hotels under 200",
            Page = 1,
            PageSize = 10
        });

        await AssertBadRequestAsync(response, "Could not extract a location");
    }

    [Fact]
    public async Task Search_returns_400_when_budget_is_negative()
    {
        _factory.PromptParserMock
            .Setup(p => p.Parse(It.IsAny<string>()))
            .Throws(new AppException("Budget cannot be negative."));

        var response = await PostSearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45, 15 under -10",
            Page = 1,
            PageSize = 10
        });

        await AssertBadRequestAsync(response, "Budget cannot be negative.");
    }

    [Fact]
    public async Task Search_returns_400_when_prompt_exceeds_max_length()
    {
        var response = await PostSearchAsync(new SearchHotelsRequestDto
        {
            Prompt = new string('a', SearchConstants.PromptMaxLength + 1),
            Page = 1,
            PageSize = 10
        });

        await AssertValidationProblemAsync(
            response,
            "prompt",
            $"Search prompt cannot exceed {SearchConstants.PromptMaxLength} characters.");
    }

    [Fact]
    public async Task Search_returns_400_when_page_or_page_size_is_invalid()
    {
        var pageResponse = await PostSearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45, 15",
            Page = 0,
            PageSize = 10
        });

        await AssertValidationProblemAsync(pageResponse, "page", "Page must be at least 1.");

        var pageSizeResponse = await PostSearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45, 15",
            Page = 1,
            PageSize = 101
        });

        await AssertValidationProblemAsync(pageSizeResponse, "pageSize", "Page size cannot exceed 100.");
    }

    [Fact]
    public async Task Search_returns_200_with_empty_items_when_no_hotels_exist()
    {
        _factory.PromptParserMock
            .Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedPrompt(45.815, 15.982, null));

        _factory.RepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var response = await PostSearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.8150, 15.9819",
            Page = 1,
            PageSize = 10
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(0, json.GetProperty("totalCount").GetInt32());
        Assert.Empty(json.GetProperty("items").EnumerateArray());
    }

    [Fact]
    public async Task CreateHotelRequestValidator_catches_invalid_name()
    {
        var validator = new CreateHotelRequestValidator();
        var result = await validator.ValidateAsync(new CreateHotelRequest { Name = "   ", Price = 100, Latitude = 45, Longitude = 15 });
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    private Task<HttpResponseMessage> PostHotelAsync(CreateHotelRequest request) =>
        _client.PostAsJsonAsync("/api/hotels", request);

    private Task<HttpResponseMessage> PostSearchAsync(SearchHotelsRequestDto request) =>
        _client.PostAsJsonAsync("/api/hotels/search", request);

    private static async Task AssertValidationProblemAsync(
        HttpResponseMessage response,
        string field,
        string expectedMessage)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Validation Failed", problem!.Title);
        Assert.NotNull(problem.Errors);

        var fieldErrors = problem.Errors
            .First(kvp => string.Equals(kvp.Key, field, StringComparison.OrdinalIgnoreCase))
            .Value;

        Assert.Contains(expectedMessage, fieldErrors);
    }

    private static async Task AssertNotFoundAsync(HttpResponseMessage response, string expectedDetail)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Not Found", problem!.Title);
        Assert.Contains(expectedDetail, problem.Detail);
    }

    private static async Task AssertBadRequestAsync(HttpResponseMessage response, string expectedDetail)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal("Bad Request", problem!.Title);
        Assert.Contains(expectedDetail, problem.Detail ?? string.Empty);
    }
}
