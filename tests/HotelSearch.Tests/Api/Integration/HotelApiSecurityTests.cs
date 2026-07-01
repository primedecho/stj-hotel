using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HotelSearch.Api.Contracts.Hotels;
using HotelSearch.Api.Contracts.Search;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Tests.Api.Integration;

[Collection(SecuredIntegrationTestCollection.Name)]
public sealed class HotelApiSecurityTests(SecuredIntegrationTestFixture fixture)
{
    [Fact]
    public async Task CreateHotel_without_api_key_returns_401_when_auth_enabled()
    {
        await fixture.ResetDatabaseAsync();

        var api = new HotelApiClient(fixture.AnonymousClient);
        var (response, _) = await api.CreateHotelAsync(SampleHotel("Secured Hotel", 100, 45.81, 15.98));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task CreateHotel_with_valid_api_key_returns_201()
    {
        await fixture.ResetDatabaseAsync();

        var api = new HotelApiClient(fixture.AuthenticatedClient);
        var (response, hotel) = await api.CreateHotelAsync(SampleHotel("Authorized Hotel", 100, 45.81, 15.98));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        hotel!.Name.Should().Be("Authorized Hotel");
    }

    [Fact]
    public async Task UpdateHotel_without_api_key_returns_401()
    {
        await fixture.ResetDatabaseAsync();

        var writeApi = new HotelApiClient(fixture.AuthenticatedClient);
        var (_, created) = await writeApi.CreateHotelAsync(SampleHotel("Update Target", 90, 45.81, 15.98));

        var anonymousApi = new HotelApiClient(fixture.AnonymousClient);
        var response = await anonymousApi.UpdateHotelAsync(created!.Id, new UpdateHotelRequest
        {
            Name = "Hacked",
            Price = 1,
            Latitude = 0,
            Longitude = 0
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteHotel_without_api_key_returns_401()
    {
        await fixture.ResetDatabaseAsync();

        var writeApi = new HotelApiClient(fixture.AuthenticatedClient);
        var (_, created) = await writeApi.CreateHotelAsync(SampleHotel("Delete Target", 90, 45.81, 15.98));

        var anonymousApi = new HotelApiClient(fixture.AnonymousClient);
        var response = await anonymousApi.DeleteHotelAsync(created!.Id);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetHotel_remains_public_without_api_key()
    {
        await fixture.ResetDatabaseAsync();

        var writeApi = new HotelApiClient(fixture.AuthenticatedClient);
        var (_, created) = await writeApi.CreateHotelAsync(SampleHotel("Public Read", 90, 45.81, 15.98));

        var anonymousApi = new HotelApiClient(fixture.AnonymousClient);
        var response = await anonymousApi.GetHotelAsync(created!.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Search_remains_public_without_api_key()
    {
        await fixture.ResetDatabaseAsync();

        var writeApi = new HotelApiClient(fixture.AuthenticatedClient);
        await writeApi.CreateHotelAsync(SampleHotel("Searchable", 90, 45.815, 15.982));

        var anonymousApi = new HotelApiClient(fixture.AnonymousClient);
        var (response, result) = await anonymousApi.SearchAsync(new SearchHotelsRequestDto
        {
            Prompt = "near 45.8150, 15.9819",
            Page = 1,
            PageSize = 10
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        result!.TotalCount.Should().Be(1);
    }

    private static CreateHotelRequest SampleHotel(string name, decimal price, double latitude, double longitude) =>
        IntegrationTestData.SampleHotel(name, price, latitude, longitude);
}
