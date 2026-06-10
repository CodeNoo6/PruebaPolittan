using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polittan.Reservations.Domain.Entities;
using Polittan.Reservations.Domain.Enums;

namespace Polittan.Reservations.Infrastructure.Persistence.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.CustomerName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Origin)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Destination)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Date)
            .IsRequired();

        builder.Property(r => r.Passengers)
            .IsRequired();

        builder.Property(r => r.ServiceType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.TotalPrice)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        // Índice compuesto para detección de duplicados eficiente
        builder.HasIndex(r => new { r.CustomerName, r.Origin, r.Destination, r.Date, r.ServiceType })
            .HasDatabaseName("IX_Reservations_Duplicate");
    }
}
