using HotelSearch.Application.Hotels.Commands;
using HotelSearch.Application.Hotels.Dtos;
using HotelSearch.Domain.Hotels;

namespace HotelSearch.Application.Hotels;

public sealed class HotelService(IHotelRepository repository) : IHotelService
{
    public async Task<HotelResponse> CreateAsync(
        CreateHotelCommand command,
        CancellationToken cancellationToken = default)
    {
        var hotel = new Hotel(
            Guid.NewGuid(),
            command.Name,
            new Money(command.Price),
            new GeoLocation(command.Latitude, command.Longitude));

        await repository.AddAsync(hotel, cancellationToken);

        return HotelMapper.ToResponse(hotel);
    }

    public async Task<HotelResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var hotel = await repository.GetByIdAsync(id, cancellationToken);

        return hotel is null ? null : HotelMapper.ToResponse(hotel);
    }

    public async Task<IReadOnlyList<HotelResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var hotels = await repository.GetAllAsync(cancellationToken);
        var responses = new List<HotelResponse>(hotels.Count);

        foreach (var hotel in hotels)
        {
            responses.Add(HotelMapper.ToResponse(hotel));
        }

        return responses;
    }

    public async Task<HotelResponse?> UpdateAsync(
        Guid id,
        UpdateHotelCommand command,
        CancellationToken cancellationToken = default)
    {
        var hotel = await repository.GetByIdForUpdateAsync(id, cancellationToken);

        if (hotel is null)
        {
            return null;
        }

        hotel.UpdateDetails(
            command.Name,
            new Money(command.Price),
            new GeoLocation(command.Latitude, command.Longitude));

        await repository.SaveChangesAsync(cancellationToken);

        return HotelMapper.ToResponse(hotel);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.DeleteAsync(id, cancellationToken);
}
