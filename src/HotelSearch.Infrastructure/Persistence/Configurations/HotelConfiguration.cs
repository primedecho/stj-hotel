using HotelSearch.Domain.Hotels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HotelSearch.Infrastructure.Persistence.Configurations;

internal sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("hotels");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(h => h.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("price")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.OwnsOne(h => h.Location, location =>
        {
            location.Property(l => l.Latitude)
                .HasColumnName("latitude")
                .IsRequired();

            location.Property(l => l.Longitude)
                .HasColumnName("longitude")
                .IsRequired();
        });

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_hotels_latitude_range",
                "latitude >= -90 AND latitude <= 90");

            t.HasCheckConstraint(
                "CK_hotels_longitude_range",
                "longitude >= -180 AND longitude <= 180");

            t.HasCheckConstraint(
                "CK_hotels_price_positive",
                "price > 0");
        });
    }
}
