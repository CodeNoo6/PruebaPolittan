using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Polittan.Reservations.Application.Services;

namespace Polittan.Reservations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IReservationService, ReservationService>();
        services.AddValidatorsFromAssemblyContaining<ReservationService>();
        return services;
    }
}
