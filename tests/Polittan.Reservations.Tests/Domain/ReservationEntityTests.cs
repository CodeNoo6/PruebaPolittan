using FluentAssertions;
using Polittan.Reservations.Domain.Entities;
using Polittan.Reservations.Domain.Enums;
using Xunit;

namespace Polittan.Reservations.Tests.Domain;

/// <summary>
/// Pruebas unitarias para la entidad Reservation.
///
/// Cubre:
///   - Estado inicial al crear (Created)
///   - Transición Created → Confirmed
///   - Transición Created → Cancelled
///   - Transición Confirmed → Cancelled
///   - Guard: no se puede confirmar una reserva ya confirmada
///   - Guard: no se puede confirmar una reserva cancelada
///   - Guard: no se puede cancelar una reserva ya cancelada
///   - Propiedades asignadas correctamente en Create
/// </summary>
public sealed class ReservationEntityTests
{
    private static Reservation BuildReservation() =>
        Reservation.Create(
            customerName: "Juan Pérez",
            origin: "Bogotá",
            destination: "Aeropuerto El Dorado",
            date: DateTime.UtcNow.AddDays(3),
            passengers: 2,
            serviceType: ServiceType.Standard,
            totalPrice: 66_500m);

    // ─── Creación ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidData_SetsStatusToCreated()
    {
        var reservation = BuildReservation();
        reservation.Status.Should().Be(ReservationStatus.Created);
    }

    [Fact]
    public void Create_ValidData_AssignsNewGuid()
    {
        var r1 = BuildReservation();
        var r2 = BuildReservation();
        r1.Id.Should().NotBe(Guid.Empty);
        r1.Id.Should().NotBe(r2.Id);
    }

    [Fact]
    public void Create_ValidData_PersistsAllProperties()
    {
        var date = DateTime.UtcNow.AddDays(3);
        var reservation = Reservation.Create("Ana", "Cali", "Cartagena", date, 3, ServiceType.Premium, 99_000m);

        reservation.CustomerName.Should().Be("Ana");
        reservation.Origin.Should().Be("Cali");
        reservation.Destination.Should().Be("Cartagena");
        reservation.Date.Should().Be(date);
        reservation.Passengers.Should().Be(3);
        reservation.ServiceType.Should().Be(ServiceType.Premium);
        reservation.TotalPrice.Should().Be(99_000m);
    }

    // ─── Confirm ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Confirm_FromCreated_ChangesStatusToConfirmed()
    {
        var reservation = BuildReservation();
        reservation.Confirm();
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_FromConfirmed_ThrowsInvalidOperationException()
    {
        var reservation = BuildReservation();
        reservation.Confirm();

        var act = () => reservation.Confirm();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Confirmed*");
    }

    [Fact]
    public void Confirm_FromCancelled_ThrowsInvalidOperationException()
    {
        var reservation = BuildReservation();
        reservation.Cancel();

        var act = () => reservation.Confirm();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cancelled*");
    }

    // ─── Cancel ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Cancel_FromCreated_ChangesStatusToCancelled()
    {
        var reservation = BuildReservation();
        reservation.Cancel();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromConfirmed_ChangesStatusToCancelled()
    {
        var reservation = BuildReservation();
        reservation.Confirm();
        reservation.Cancel();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var reservation = BuildReservation();
        reservation.Cancel();

        var act = () => reservation.Cancel();
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already cancelled*");
    }
}
