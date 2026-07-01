using HotelSearch.Application.Hotels;
using HotelSearch.Domain.Hotels;
using Microsoft.EntityFrameworkCore;

namespace HotelSearch.Infrastructure.Persistence.Repositories;

internal sealed class HotelRepository(HotelSearchDbContext context) : IHotelRepository
{
    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Hotels
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<Hotel?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Hotels.FirstOrDefaultAsync(h => h.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Hotel>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Hotels
            .AsNoTracking()
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        await context.Hotels.AddAsync(hotel, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await context.Hotels
            .Where(h => h.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        return deleted > 0;
    }
}
