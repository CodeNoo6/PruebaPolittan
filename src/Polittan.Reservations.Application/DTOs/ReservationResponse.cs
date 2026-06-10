namespace Polittan.Reservations.Application.DTOs;

public sealed record ReservationResponse(
    Guid Id,
    string CustomerName,
    string Origin,
    string Destination,
    DateTime Date,
    int Passengers,
    string ServiceType,
    string Status,
    decimal TotalPrice,
    DateTime CreatedAt
);
