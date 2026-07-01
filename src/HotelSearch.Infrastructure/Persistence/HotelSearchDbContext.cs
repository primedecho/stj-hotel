using HotelSearch.Domain.Hotels;
using Microsoft.EntityFrameworkCore;

namespace HotelSearch.Infrastructure.Persistence;

public sealed class HotelSearchDbContext : DbContext
{
    public HotelSearchDbContext(DbContextOptions<HotelSearchDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels => Set<Hotel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelSearchDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
