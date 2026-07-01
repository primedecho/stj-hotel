using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelSearch.Infrastructure.Persistence;

public sealed class HotelSearchDbContextFactory : IDesignTimeDbContextFactory<HotelSearchDbContext>
{
    public HotelSearchDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<HotelSearchDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=hotels;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new HotelSearchDbContext(optionsBuilder.Options);
    }
}
