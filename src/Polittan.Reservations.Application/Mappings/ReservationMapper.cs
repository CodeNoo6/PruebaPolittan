using Polittan.Reservations.Application.DTOs;
using Polittan.Reservations.Domain.Entities;

namespace Polittan.Reservations.Application.Mappings;

public static class ReservationMapper
{
    public static ReservationResponse ToResponse(this Reservation r) => new(
        r.Id,
        r.CustomerName,
        r.Origin,
        r.Destination,
        r.Date,
        r.Passengers,
        r.ServiceType.ToString(),
        r.Status.ToString(),
        r.TotalPrice,
        r.CreatedAt
    );
}
