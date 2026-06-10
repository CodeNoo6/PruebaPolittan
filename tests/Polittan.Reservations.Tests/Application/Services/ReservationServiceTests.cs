using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using Polittan.Reservations.Application.DTOs;
using Polittan.Reservations.Application.Services;
using Polittan.Reservations.Domain.Entities;
using Polittan.Reservations.Domain.Enums;
using Polittan.Reservations.Domain.Exceptions;
using Polittan.Reservations.Domain.Interfaces;

namespace Polittan.Reservations.Tests.Application.Services;

/// <summary>
/// Pruebas unitarias para ReservationService.
///
/// Usa NSubstitute para aislar el servicio del repositorio real.
///
/// Cubre:
///   - CreateAsync: caso feliz → retorna respuesta con estado Created
///   - CreateAsync: duplicado → lanza DomainException
///   - GetAllAsync: retorna lista mapeada
///   - GetByIdAsync: encontrado → retorna respuesta
///   - GetByIdAsync: no encontrado → lanza NotFoundException
///   - ConfirmAsync: caso feliz → estado Confirmed
///   - ConfirmAsync: no encontrado → lanza NotFoundException
///   - CancelAsync: caso feliz → estado Cancelled
///   - CancelAsync: no encontrado → lanza NotFoundException
/// </summary>
public sealed class ReservationServiceTests
{
    private readonly IReservationRepository _repository = Substitute.For<IReservationRepository>();
    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        _service = new ReservationService(_repository);
    }

    private static CreateReservationRequest ValidRequest(int daysAhead = 3) => new(
        CustomerName: "Juan Pérez",
        Origin: "Bogotá",
        Destination: "Aeropuerto El Dorado",
        Date: DateTime.UtcNow.AddDays(daysAhead),
        Passengers: 2,
        ServiceType: "standard"
    );

    private static Reservation BuildSavedReservation(ReservationStatus status = ReservationStatus.Created)
    {
        var r = Reservation.Create("Juan Pérez", "Bogotá", "Aeropuerto El Dorado",
            DateTime.UtcNow.AddDays(3), 2, ServiceType.Standard, 66_500m);
        if (status == ReservationStatus.Confirmed) r.Confirm();
        if (status == ReservationStatus.Cancelled) r.Cancel();
        return r;
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedReservation()
    {
        _repository.ExistsDuplicateAsync(Arg.Any<Reservation>()).Returns(false);

        var response = await _service.CreateAsync(ValidRequest());

        response.Status.Should().Be(nameof(ReservationStatus.Created));
        response.CustomerName.Should().Be("Juan Pérez");
        response.ServiceType.Should().Be(nameof(ServiceType.Standard));
        await _repository.Received(1).AddAsync(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_PriceIsPositive()
    {
        _repository.ExistsDuplicateAsync(Arg.Any<Reservation>()).Returns(false);

        var response = await _service.CreateAsync(ValidRequest());

        response.TotalPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_DuplicateExists_ThrowsDomainException()
    {
        _repository.ExistsDuplicateAsync(Arg.Any<Reservation>()).Returns(true);

        var act = () => _service.CreateAsync(ValidRequest());

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
        await _repository.DidNotReceive().AddAsync(Arg.Any<Reservation>());
    }

    [Fact]
    public async Task CreateAsync_EarlyBooking_AppliesDiscount()
    {
        _repository.ExistsDuplicateAsync(Arg.Any<Reservation>()).Returns(false);

        var earlyResponse = await _service.CreateAsync(ValidRequest(daysAhead: 5));

        // Con descuento anticipado el precio debe ser menor que sin descuento (1 día)
        _repository.ExistsDuplicateAsync(Arg.Any<Reservation>()).Returns(false);
        var lateResponse = await _service.CreateAsync(ValidRequest(daysAhead: 1));

        earlyResponse.TotalPrice.Should().BeLessThan(lateResponse.TotalPrice);
    }

    // ─── GetAllAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllReservationsMapped()
    {
        var reservations = new List<Reservation>
        {
            BuildSavedReservation(),
            BuildSavedReservation()
        };
        _repository.GetAllAsync().Returns(reservations);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.Status.Should().Be(nameof(ReservationStatus.Created)));
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        _repository.GetAllAsync().Returns(new List<Reservation>());

        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsReservation()
    {
        var reservation = BuildSavedReservation();
        _repository.GetByIdAsync(reservation.Id).Returns(reservation);

        var result = await _service.GetByIdAsync(reservation.Id);

        result.Id.Should().Be(reservation.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Reservation?)null);

        var act = () => _service.GetByIdAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // ─── ConfirmAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmAsync_CreatedReservation_ReturnsConfirmedStatus()
    {
        var reservation = BuildSavedReservation();
        _repository.GetByIdAsync(reservation.Id).Returns(reservation);

        var result = await _service.ConfirmAsync(reservation.Id);

        result.Status.Should().Be(nameof(ReservationStatus.Confirmed));
    }

    [Fact]
    public async Task ConfirmAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Reservation?)null);

        var act = () => _service.ConfirmAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ConfirmAsync_AlreadyConfirmed_ThrowsInvalidOperationException()
    {
        var reservation = BuildSavedReservation(ReservationStatus.Confirmed);
        _repository.GetByIdAsync(reservation.Id).Returns(reservation);

        var act = () => _service.ConfirmAsync(reservation.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ─── CancelAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_CreatedReservation_ReturnsCancelledStatus()
    {
        var reservation = BuildSavedReservation();
        _repository.GetByIdAsync(reservation.Id).Returns(reservation);

        var result = await _service.CancelAsync(reservation.Id);

        result.Status.Should().Be(nameof(ReservationStatus.Cancelled));
    }

    [Fact]
    public async Task CancelAsync_ConfirmedReservation_ReturnsCancelledStatus()
    {
        var reservation = BuildSavedReservation(ReservationStatus.Confirmed);
        _repository.GetByIdAsync(reservation.Id).Returns(reservation);

        var result = await _service.CancelAsync(reservation.Id);

        result.Status.Should().Be(nameof(ReservationStatus.Cancelled));
    }

    [Fact]
    public async Task CancelAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>()).Returns((Reservation?)null);

        var act = () => _service.CancelAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelled_ThrowsInvalidOperationException()
    {
        var reservation = BuildSavedReservation(ReservationStatus.Cancelled);
        _repository.GetByIdAsync(reservation.Id).Returns(reservation);

        var act = () => _service.CancelAsync(reservation.Id);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
