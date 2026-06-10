using Polittan.Reservations.Application.DTOs;
using Polittan.Reservations.Application.Mappings;
using Polittan.Reservations.Application.Pricing;
using Polittan.Reservations.Domain.Entities;
using Polittan.Reservations.Domain.Enums;
using Polittan.Reservations.Domain.Exceptions;
using Polittan.Reservations.Domain.Interfaces;

namespace Polittan.Reservations.Application.Services;

public sealed class ReservationService(IReservationRepository repository) : IReservationService
{
    public async Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct = default)
    {
        var serviceType = Enum.Parse<ServiceType>(request.ServiceType, ignoreCase: true);
        var now = DateTime.UtcNow;

        var price = PricingRules.Calculate(serviceType, request.Passengers, request.Date, now);

        var reservation = Reservation.Create(
            request.CustomerName,
            request.Origin,
            request.Destination,
            request.Date,
            request.Passengers,
            serviceType,
            price);

        if (await repository.ExistsDuplicateAsync(reservation, ct))
            throw new DomainException("A reservation with the same customer, route, date and service type already exists.");

        await repository.AddAsync(reservation, ct);

        return reservation.ToResponse();
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var reservations = await repository.GetAllAsync(ct);
        return [.. reservations.Select(r => r.ToResponse())];
    }

    public async Task<ReservationResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Reservation '{id}' not found.");

        return reservation.ToResponse();
    }

    public async Task<ReservationResponse> ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Reservation '{id}' not found.");

        reservation.Confirm();
        return reservation.ToResponse();
    }

    public async Task<ReservationResponse> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await repository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Reservation '{id}' not found.");

        reservation.Cancel();
        return reservation.ToResponse();
    }
}
