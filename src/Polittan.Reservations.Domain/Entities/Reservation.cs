using Polittan.Reservations.Domain.Enums;

namespace Polittan.Reservations.Domain.Entities;

public sealed class Reservation
{
    public Guid Id { get; private set; }
    public string CustomerName { get; private set; } = string.Empty;
    public string Origin { get; private set; } = string.Empty;
    public string Destination { get; private set; } = string.Empty;
    public DateTime Date { get; private set; }
    public int Passengers { get; private set; }
    public ServiceType ServiceType { get; private set; }
    public ReservationStatus Status { get; private set; }
    public decimal TotalPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Reservation() { }

    public static Reservation Create(
        string customerName,
        string origin,
        string destination,
        DateTime date,
        int passengers,
        ServiceType serviceType,
        decimal totalPrice)
    {
        return new Reservation
        {
            Id = Guid.NewGuid(),
            CustomerName = customerName,
            Origin = origin,
            Destination = destination,
            Date = date,
            Passengers = passengers,
            ServiceType = serviceType,
            Status = ReservationStatus.Created,
            TotalPrice = totalPrice,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Created)
            throw new InvalidOperationException($"Cannot confirm a reservation in '{Status}' status.");

        Status = ReservationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
            throw new InvalidOperationException("Reservation is already cancelled.");

        Status = ReservationStatus.Cancelled;
    }
}
