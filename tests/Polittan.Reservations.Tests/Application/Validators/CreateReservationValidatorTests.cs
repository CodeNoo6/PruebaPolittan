using FluentAssertions;
using Polittan.Reservations.Application.DTOs;
using Polittan.Reservations.Application.Validators;
using Xunit;

namespace Polittan.Reservations.Tests.Application.Validators;

/// <summary>
/// Pruebas unitarias para CreateReservationValidator.
///
/// Cubre cada regla de validación por separado y el caso feliz completo:
///   - CustomerName obligatorio
///   - Origin y Destination obligatorios
///   - Origin ≠ Destination (case-insensitive)
///   - Date debe ser futura
///   - Passengers entre 1 y 6
///   - ServiceType solo "standard" o "premium" (case-insensitive)
///   - Request válido no produce errores
/// </summary>
public sealed class CreateReservationValidatorTests
{
    private readonly CreateReservationValidator _validator = new();

    private static CreateReservationRequest ValidRequest() => new(
        CustomerName: "Juan Pérez",
        Origin: "Bogotá",
        Destination: "Aeropuerto El Dorado",
        Date: DateTime.UtcNow.AddDays(3),
        Passengers: 2,
        ServiceType: "standard"
    );

    // ─── Caso feliz ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_ValidRequest_ReturnsNoErrors()
    {
        var result = await _validator.ValidateAsync(ValidRequest());
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("standard")]
    [InlineData("Standard")]
    [InlineData("STANDARD")]
    [InlineData("premium")]
    [InlineData("Premium")]
    [InlineData("PREMIUM")]
    public async Task Validate_ServiceType_CaseInsensitive_IsValid(string serviceType)
    {
        var request = ValidRequest() with { ServiceType = serviceType };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    // ─── CustomerName ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validate_CustomerName_Empty_FailsValidation(string? name)
    {
        var request = ValidRequest() with { CustomerName = name! };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.CustomerName));
    }

    // ─── Origin ──────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_Origin_Empty_FailsValidation(string? origin)
    {
        var request = ValidRequest() with { Origin = origin! };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Origin));
    }

    // ─── Destination ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validate_Destination_Empty_FailsValidation(string? destination)
    {
        var request = ValidRequest() with { Destination = destination! };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Destination));
    }

    // ─── Origin ≠ Destination ────────────────────────────────────────────────────

    [Theory]
    [InlineData("Bogotá", "Bogotá")]
    [InlineData("bogotá", "BOGOTÁ")]
    [InlineData("aeropuerto", "Aeropuerto")]
    public async Task Validate_OriginEqualsDestination_FailsValidation(string origin, string destination)
    {
        var request = ValidRequest() with { Origin = origin, Destination = destination };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Origin));
    }

    // ─── Date ────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Validate_DateInThePast_FailsValidation()
    {
        var request = ValidRequest() with { Date = DateTime.UtcNow.AddDays(-1) };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Date));
    }

    [Fact]
    public async Task Validate_DateIsNow_FailsValidation()
    {
        // Retrocedemos 1 segundo para garantizar que no es futura en el momento de validar
        var request = ValidRequest() with { Date = DateTime.UtcNow.AddSeconds(-1) };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }

    // ─── Passengers ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(7)]
    [InlineData(100)]
    public async Task Validate_PassengersOutOfRange_FailsValidation(int passengers)
    {
        var request = ValidRequest() with { Passengers = passengers };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Passengers));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(6)]
    public async Task Validate_PassengersWithinRange_IsValid(int passengers)
    {
        var request = ValidRequest() with { Passengers = passengers };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    // ─── ServiceType ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("express")]
    [InlineData("vip")]
    [InlineData("123")]
    public async Task Validate_ServiceType_Invalid_FailsValidation(string? serviceType)
    {
        var request = ValidRequest() with { ServiceType = serviceType! };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.ServiceType));
    }
}
