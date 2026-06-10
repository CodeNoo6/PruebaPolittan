using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polittan.Reservations.Domain.Interfaces;
using Polittan.Reservations.Infrastructure.Persistence;
using Polittan.Reservations.Infrastructure.Repositories;

namespace Polittan.Reservations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=reservations.db";

        services.AddDbContext<ReservationsDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IReservationRepository, EfReservationRepository>();

        return services;
    }
}
