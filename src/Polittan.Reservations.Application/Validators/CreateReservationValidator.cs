using FluentValidation;
using Polittan.Reservations.Application.DTOs;

namespace Polittan.Reservations.Application.Validators;

public sealed class CreateReservationValidator : AbstractValidator<CreateReservationRequest>
{
    private static readonly string[] AllowedServiceTypes = ["standard", "premium"];

    public CreateReservationValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("CustomerName is required.")
            .MaximumLength(150);

        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required.")
            .MaximumLength(200);

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.")
            .MaximumLength(200);

        RuleFor(x => x.Origin)
            .NotEqual(x => x.Destination, StringComparer.OrdinalIgnoreCase)
            .WithMessage("Origin and Destination must be different.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.")
            .GreaterThan(DateTime.UtcNow).WithMessage("Date must be in the future.");

        RuleFor(x => x.Passengers)
            .InclusiveBetween(1, 6).WithMessage("Passengers must be between 1 and 6.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("ServiceType is required.")
            .Must(s => AllowedServiceTypes.Contains(s?.ToLower()))
            .WithMessage("ServiceType must be 'standard' or 'premium'.");
    }
}
