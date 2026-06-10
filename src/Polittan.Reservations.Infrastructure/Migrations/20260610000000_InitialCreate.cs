using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polittan.Reservations.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Reservations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                CustomerName = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                Origin = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Destination = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                Passengers = table.Column<int>(type: "INTEGER", nullable: false),
                ServiceType = table.Column<string>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                TotalPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reservations", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Reservations_Duplicate",
            table: "Reservations",
            columns: new[] { "CustomerName", "Origin", "Destination", "Date", "ServiceType" });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Reservations");
    }
}
