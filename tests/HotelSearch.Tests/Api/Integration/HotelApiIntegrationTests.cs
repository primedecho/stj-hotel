using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HotelSearch.Api.Contracts.Hotels;
using HotelSearch.Api.Contracts.Search;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Tests.Api.Integration;

[Collection(IntegrationTestCollection.Name)]
public sealed class HotelApiIntegrationTests(IntegrationTestFixture fixture)
{
    private readonly HotelApiClient _api = new(fixture.Client);

    [Fact]
    public async Task CreateHotel_returns_201_with_location_and_body()
    {
        await fixture.ResetDatabaseAsync();

        var request = new CreateHotelRequest
        {
            Name = "Grand Hotel Zagreb",
            Price = 120,
            Latitude = 45.8150,
            Longitude = 15.9819
        };

        var (response, hotel) = await _api.CreateHotelAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.OriginalString.Should().Be($"/api/hotels/{hotel!.Id}");

        hotel.Name.Should().Be(request.Name);
        hotel.Price.Should().Be(request.Price);
        hotel.Latitude.Should().Be(request.Latitude);
        hotel.Longitude.Should().Be(request.Longitude);
    }

    [Fact]
    public async Task GetHotel_returns_persisted_hotel()
    {
        await fixture.ResetDatabaseAsync();

        var (_, created) = await _api.CreateHotelAsync(SampleHotel("Get Test Hotel", 99, 45.81, 15.98));

        var response = await _api.GetHotelAsync(created!.Id);
        var hotel = await response.Content.ReadFromJsonAsync<HotelDto>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        hotel!.Id.Should().Be(created.Id);
        hotel.Name.Should().Be("Get Test Hotel");
    }

    [Fact]
    public async Task ListHotels_returns_all_persisted_hotels()
    {
        await fixture.ResetDatabaseAsync();

        await _api.CreateHotelAsync(SampleHotel("Hotel Alpha", 80, 45.81, 15.98));
        await _api.CreateHotelAsync(SampleHotel("Hotel Beta", 110, 45.82, 15.99));

        var hotels = await _api.ListHotelsAsync();

        hotels.Should().HaveCount(2);
        hotels.Select(h => h.Name).Should().BeEquivalentTo(["Hotel Alpha", "Hotel Beta"]);
    }

    [Fact]
    public async Task UpdateHotel_persists_changes()
    {
        await fixture.ResetDatabaseAsync();

        var (_, created) = await _api.CreateHotelAsync(SampleHotel("Before Update", 90, 45.81, 15.98));

        var updateResponse = await _api.UpdateHotelAsync(created!.Id, new UpdateHotelRequest
        {
            Name = "After Update",
            Price = 150,
            Latitude = 45.82,
            Longitude = 15.99
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _api.GetHotelAsync(created.Id);
        var updated = await getResponse.Content.ReadFromJsonAsync<HotelDto>();

        updated!.Name.Should().Be("After Update");
        updated.Price.Should().Be(150);
        updated.Latitude.Should().Be(45.82);
        updated.Longitude.Should().Be(15.99);
    }

    [Fact]
    public async Task DeleteHotel_removes_hotel()
    {
        await fixture.ResetDatabaseAsync();

        var (_, created) = await _api.CreateHotelAsync(SampleHotel("To Delete", 75, 45.81, 15.98));

        var deleteResponse = await _api.DeleteHotelAsync(created!.Id);
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _api.GetHotelAsync(created.Id);
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Search_returns_only_hotels_created_through_api()
    {
        await fixture.ResetDatabaseAsync();

        await _api.CreateHotelAsync(SampleHotel("Stored Hotel A", 100, 45.815, 15.982));
        await _api.CreateHotelAsync(SampleHotel("Stored Hotel B", 140, 45.820, 15.990));

        var (_, result) = await _api.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.8150, 15.9819",
            Page = 1,
            PageSize = 10
        });

        result!.TotalCount.Should().Be(2);
        result.Items.Select(i => i.Name).Should().BeEquivalentTo(["Stored Hotel A", "Stored Hotel B"]);
    }

    [Fact]
    public async Task Search_orders_cheaper_and_closer_hotels_first()
    {
        await fixture.ResetDatabaseAsync();

        await _api.CreateHotelAsync(SampleHotel("Expensive Far", 300, 46.0, 16.0));
        await _api.CreateHotelAsync(SampleHotel("Budget Near", 50, 45.001, 15.001));

        var (_, result) = await _api.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.0, 15.0",
            Page = 1,
            PageSize = 10
        });

        result!.Items.Should().HaveCount(2);
        result.Items[0].Name.Should().Be("Budget Near");
        result.Items[1].Name.Should().Be("Expensive Far");
        result.Items[0].RankingScore.Should().BeLessThan(result.Items[1].RankingScore);
    }

    [Fact]
    public async Task Search_applies_paging()
    {
        await fixture.ResetDatabaseAsync();

        for (var i = 1; i <= 5; i++)
        {
            await _api.CreateHotelAsync(SampleHotel($"Paged Hotel {i}", 50 + i, 45.0 + i * 0.001, 15.0));
        }

        var (_, page1) = await _api.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.0, 15.0",
            Page = 1,
            PageSize = 2
        });

        var (_, page2) = await _api.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.0, 15.0",
            Page = 2,
            PageSize = 2
        });

        page1!.Items.Should().HaveCount(2);
        page1.TotalCount.Should().Be(5);
        page1.TotalPages.Should().Be(3);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);

        page2!.Items.Should().HaveCount(2);
        page2.Page.Should().Be(2);

        page1.Items.Select(i => i.Name)
            .Should().NotIntersectWith(page2.Items.Select(i => i.Name));
    }

    [Fact]
    public async Task CreateHotel_with_invalid_request_returns_400()
    {
        await fixture.ResetDatabaseAsync();

        var (response, _) = await _api.CreateHotelAsync(new CreateHotelRequest
        {
            Name = "   ",
            Price = 0,
            Latitude = 45,
            Longitude = 15
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem!.Title.Should().Be("Validation Failed");
        problem.Errors.Keys.Should().Contain(k => string.Equals(k, "Name", StringComparison.OrdinalIgnoreCase));
        problem.Errors.Keys.Should().Contain(k => string.Equals(k, "Price", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Search_with_invalid_prompt_returns_400()
    {
        await fixture.ResetDatabaseAsync();

        var (response, _) = await _api.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "hotels under 200",
            Page = 1,
            PageSize = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("Bad Request");
        problem.Detail.Should().Contain("Could not extract a location");
    }

    [Fact]
    public async Task GetHotel_for_non_existing_id_returns_404()
    {
        await fixture.ResetDatabaseAsync();

        var id = Guid.NewGuid();
        var response = await _api.GetHotelAsync(id);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("Not Found");
        problem.Detail.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task Search_with_no_hotels_returns_empty_page()
    {
        await fixture.ResetDatabaseAsync();

        var (_, result) = await _api.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.8150, 15.9819",
            Page = 1,
            PageSize = 10
        });

        result!.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
        result.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Health_returns_healthy_with_database_status()
    {
        var response = await fixture.Client.GetAsync("/health");
        var health = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        health!.Status.Should().Be("healthy");
        health.Database.Should().Be("healthy");
    }

    private sealed record HealthCheckResponse(string Status, string Database);

    private static CreateHotelRequest SampleHotel(string name, decimal price, double latitude, double longitude) =>
        IntegrationTestData.SampleHotel(name, price, latitude, longitude);
}
