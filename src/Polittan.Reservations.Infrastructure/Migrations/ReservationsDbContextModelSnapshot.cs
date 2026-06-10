using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Polittan.Reservations.Infrastructure.Persistence;

#nullable disable

namespace Polittan.Reservations.Infrastructure.Migrations;

[DbContext(typeof(ReservationsDbContext))]
partial class ReservationsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity("Polittan.Reservations.Domain.Entities.Reservation", b =>
        {
            b.Property<Guid>("Id")
                .HasColumnType("TEXT");

            b.Property<DateTime>("CreatedAt")
                .HasColumnType("TEXT");

            b.Property<DateTime>("Date")
                .HasColumnType("TEXT");

            b.Property<string>("CustomerName")
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnType("TEXT");

            b.Property<string>("Destination")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("TEXT");

            b.Property<string>("Origin")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("TEXT");

            b.Property<int>("Passengers")
                .HasColumnType("INTEGER");

            b.Property<string>("ServiceType")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<string>("Status")
                .IsRequired()
                .HasColumnType("TEXT");

            b.Property<decimal>("TotalPrice")
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.HasIndex(new[] { "CustomerName", "Origin", "Destination", "Date", "ServiceType" }, "IX_Reservations_Duplicate");

            b.ToTable("Reservations");
        });
#pragma warning restore 612, 618
    }
}
