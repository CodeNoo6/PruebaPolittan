namespace Polittan.Reservations.Application.DTOs;

public sealed record CreateReservationRequest(
    string CustomerName,
    string Origin,
    string Destination,
    DateTime Date,
    int Passengers,
    string ServiceType
);
