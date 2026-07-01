using HotelSearch.Application.Hotels.Commands;
using HotelSearch.Application.Hotels.Dtos;

namespace HotelSearch.Application.Hotels;

public interface IHotelService
{
    Task<HotelResponse> CreateAsync(CreateHotelCommand command, CancellationToken cancellationToken = default);

    Task<HotelResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HotelResponse>> ListAsync(CancellationToken cancellationToken = default);

    Task<HotelResponse?> UpdateAsync(Guid id, UpdateHotelCommand command, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
