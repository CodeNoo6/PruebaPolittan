using FluentAssertions;
using Polittan.Reservations.Application.Pricing;
using Polittan.Reservations.Domain.Enums;
using Xunit;

namespace Polittan.Reservations.Tests.Pricing;

/// <summary>
/// Pruebas unitarias para PricingRules.
///
/// Nomenclatura: MetodoAProbar_Escenario_ResultadoEsperado
///
/// Reglas cubiertas:
///   - Base standard (50.000) y premium (80.000)
///   - +10.000 COP por pasajero
///   - +20% si es el mismo día
///   - +15% si hay más de 4 pasajeros
///   - +10% adicional si es premium y hay más de 3 pasajeros
///   - -5% si la reserva es con 2+ días de anticipación
///   - Combinaciones de reglas (acumulativas)
/// </summary>
public sealed class PricingRulesTests
{
    // Fecha base fija para todos los tests que no necesitan "mismo día"
    private static readonly DateTime Now = new(2026, 6, 10, 12, 0, 0, DateTimeKind.Utc);

    // ─── Precio base ────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_StandardService_1Passenger_FutureDate_ReturnsBaseRate()
    {
        // Base: 50.000 + (1 × 10.000) = 60.000  →  sin recargos ni descuentos
        var date = Now.AddDays(1); // mañana: ni mismo día ni 2+ días
        var result = PricingRules.Calculate(ServiceType.Standard, 1, date, Now);
        result.Should().Be(60_000m);
    }

    [Fact]
    public void Calculate_PremiumService_1Passenger_FutureDate_ReturnsBaseRate()
    {
        // Base: 80.000 + (1 × 10.000) = 90.000
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Premium, 1, date, Now);
        result.Should().Be(90_000m);
    }

    // ─── Por pasajero ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(1, 60_000)]
    [InlineData(2, 70_000)]
    [InlineData(3, 80_000)]
    [InlineData(4, 90_000)]
    public void Calculate_StandardService_VaryingPassengers_NoSurcharges_ReturnsCorrectBase(int passengers, decimal expected)
    {
        // Fecha a 1 día: sin recargo de mismo día, sin descuento anticipado
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Standard, passengers, date, Now);
        result.Should().Be(expected);
    }

    // ─── Mismo día (+20%) ────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_StandardService_SameDay_AppliesTwentyPercentSurcharge()
    {
        // (50.000 + 2×10.000) × 1.20 = 70.000 × 1.20 = 84.000
        var result = PricingRules.Calculate(ServiceType.Standard, 2, Now, Now);
        result.Should().Be(84_000m);
    }

    [Fact]
    public void Calculate_PremiumService_SameDay_AppliesTwentyPercentSurcharge()
    {
        // (80.000 + 1×10.000) × 1.20 = 90.000 × 1.20 = 108.000
        var result = PricingRules.Calculate(ServiceType.Premium, 1, Now, Now);
        result.Should().Be(108_000m);
    }

    // ─── Más de 4 pasajeros (+15%) ───────────────────────────────────────────────

    [Fact]
    public void Calculate_StandardService_FivePassengers_AppliesFifteenPercentSurcharge()
    {
        // (50.000 + 5×10.000) × 1.15 = 100.000 × 1.15 = 115.000
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Standard, 5, date, Now);
        result.Should().Be(115_000m);
    }

    [Fact]
    public void Calculate_FourPassengers_DoesNotApplyLargeGroupSurcharge()
    {
        // Exactamente 4 pasajeros: sin recargo de grupo grande
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Standard, 4, date, Now);
        result.Should().Be(90_000m);
    }

    // ─── Premium + más de 3 pasajeros (+10%) ─────────────────────────────────────

    [Fact]
    public void Calculate_PremiumService_FourPassengers_AppliesPremiumGroupSurcharge()
    {
        // (80.000 + 4×10.000) × 1.10 = 120.000 × 1.10 = 132.000
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Premium, 4, date, Now);
        result.Should().Be(132_000m);
    }

    [Fact]
    public void Calculate_StandardService_FourPassengers_DoesNotApplyPremiumGroupSurcharge()
    {
        // Standard nunca aplica el recargo premium+grupo
        var date = Now.AddDays(1);
        var standard = PricingRules.Calculate(ServiceType.Standard, 4, date, Now);
        var premium = PricingRules.Calculate(ServiceType.Premium, 4, date, Now);
        standard.Should().BeLessThan(premium);
    }

    [Fact]
    public void Calculate_PremiumService_ThreePassengers_DoesNotApplyPremiumGroupSurcharge()
    {
        // Exactamente 3 pasajeros: sin recargo premium+grupo
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Premium, 3, date, Now);
        result.Should().Be(110_000m); // (80.000 + 3×10.000) = 110.000, sin recargo
    }

    // ─── Descuento anticipado (-5%, 2+ días) ─────────────────────────────────────

    [Fact]
    public void Calculate_StandardService_TwoDaysAhead_AppliesFivePercentDiscount()
    {
        // (50.000 + 2×10.000) × 0.95 = 70.000 × 0.95 = 66.500
        var date = Now.AddDays(2);
        var result = PricingRules.Calculate(ServiceType.Standard, 2, date, Now);
        result.Should().Be(66_500m);
    }

    [Fact]
    public void Calculate_OneDayAhead_DoesNotApplyEarlyBookingDiscount()
    {
        // 1 día: ni recargo ni descuento
        var date = Now.AddDays(1);
        var result = PricingRules.Calculate(ServiceType.Standard, 2, date, Now);
        result.Should().Be(70_000m);
    }

    // ─── Combinaciones ───────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_Premium_FivePassengers_SameDay_AppliesAllSurcharges()
    {
        // Base:  80.000 + 5×10.000 = 130.000
        // +20% mismo día:           130.000 × 1.20 = 156.000
        // +15% >4 pasajeros:        156.000 × 1.15 = 179.400
        // +10% premium+>3 pass:     179.400 × 1.10 = 197.340
        var result = PricingRules.Calculate(ServiceType.Premium, 5, Now, Now);
        result.Should().Be(197_340m);
    }

    [Fact]
    public void Calculate_Premium_FivePassengers_TwoDaysAhead_AppliesSurchargesAndDiscount()
    {
        // Base:  80.000 + 5×10.000 = 130.000
        // +15% >4 pasajeros:        130.000 × 1.15 = 149.500
        // +10% premium+>3 pass:     149.500 × 1.10 = 164.450
        // −5%  anticipado:          164.450 × 0.95 = 156.227,50
        var date = Now.AddDays(2);
        var result = PricingRules.Calculate(ServiceType.Premium, 5, date, Now);
        result.Should().Be(156_227.50m);
    }

    [Fact]
    public void Calculate_Standard_SixPassengers_SameDay_NoEarlyDiscount()
    {
        // Base:  50.000 + 6×10.000 = 110.000
        // +20% mismo día:           110.000 × 1.20 = 132.000
        // +15% >4 pasajeros:        132.000 × 1.15 = 151.800
        var result = PricingRules.Calculate(ServiceType.Standard, 6, Now, Now);
        result.Should().Be(151_800m);
    }

    [Fact]
    public void Calculate_ReturnsRoundedTwoDecimals()
    {
        // Cualquier precio debe tener máximo 2 decimales
        var result = PricingRules.Calculate(ServiceType.Premium, 5, Now.AddDays(2), Now);
        var decimals = BitConverter.GetBytes(decimal.GetBits(result)[3])[2];
        decimals.Should().BeLessThanOrEqualTo(2);
    }
}
