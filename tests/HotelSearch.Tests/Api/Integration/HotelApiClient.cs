using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using HotelSearch.Api.Contracts.Hotels;
using HotelSearch.Api.Contracts.Search;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Tests.Api.Integration;

public sealed class HotelApiClient
{
    private readonly HttpClient _client;

    public HotelApiClient(HttpClient client) => _client = client;

    public async Task<(HttpResponseMessage Response, HotelDto? Hotel)> CreateHotelAsync(CreateHotelRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/hotels", request);
        var hotel = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<HotelDto>()
            : null;

        return (response, hotel);
    }

    public Task<HttpResponseMessage> GetHotelAsync(Guid id) =>
        _client.GetAsync($"/api/hotels/{id}");

    public async Task<IReadOnlyList<HotelDto>> ListHotelsAsync()
    {
        var response = await _client.GetAsync("/api/hotels");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<List<HotelDto>>())!;
    }

    public Task<HttpResponseMessage> UpdateHotelAsync(Guid id, UpdateHotelRequest request) =>
        _client.PutAsJsonAsync($"/api/hotels/{id}", request);

    public Task<HttpResponseMessage> DeleteHotelAsync(Guid id) =>
        _client.DeleteAsync($"/api/hotels/{id}");

    public async Task<(HttpResponseMessage Response, SearchResponseDto? Result)> SearchAsync(SearchHotelsRequestDto request)
    {
        var response = await _client.PostAsJsonAsync("/api/hotels/search", request);
        var result = response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<SearchResponseDto>()
            : null;

        return (response, result);
    }
}

public sealed record SearchResponseDto(
    IReadOnlyList<SearchHotelItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
