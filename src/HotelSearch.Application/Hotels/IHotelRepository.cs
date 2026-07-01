using HotelSearch.Domain.Hotels;

namespace HotelSearch.Application.Hotels;

public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Hotel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
