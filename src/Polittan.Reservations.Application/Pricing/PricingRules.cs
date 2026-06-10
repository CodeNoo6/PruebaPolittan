using Polittan.Reservations.Domain.Enums;

namespace Polittan.Reservations.Application.Pricing;

public static class PricingRules
{
    private const decimal StandardBase = 50_000m;
    private const decimal PremiumBase = 80_000m;
    private const decimal PerPassengerFee = 10_000m;
    private const decimal SameDaySurcharge = 0.20m;
    private const decimal LargeGroupSurcharge = 0.15m;      // > 4 passengers
    private const decimal PremiumLargeGroupSurcharge = 0.10m; // premium + > 3 passengers
    private const decimal EarlyBookingDiscount = 0.05m;     // 2+ days in advance

    public static decimal Calculate(ServiceType serviceType, int passengers, DateTime reservationDate, DateTime now)
    {
        decimal price = serviceType == ServiceType.Premium ? PremiumBase : StandardBase;

        price += passengers * PerPassengerFee;

        var daysUntilTrip = (reservationDate.Date - now.Date).TotalDays;

        if (daysUntilTrip == 0)
            price *= 1 + SameDaySurcharge;

        if (passengers > 4)
            price *= 1 + LargeGroupSurcharge;

        if (serviceType == ServiceType.Premium && passengers > 3)
            price *= 1 + PremiumLargeGroupSurcharge;

        if (daysUntilTrip >= 2)
            price *= 1 - EarlyBookingDiscount;

        return Math.Round(price, 2);
    }
}
