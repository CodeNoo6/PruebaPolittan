using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Polittan.Reservations.Application.DTOs;
using Polittan.Reservations.Application.Services;

namespace Polittan.Reservations.API.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reservations")
            .WithTags("Reservations");

        group.MapPost("/", CreateReservation)
            .WithName("CreateReservation")
            .WithSummary("Create a new transfer reservation")
            .Produces<ReservationResponse>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/", GetAllReservations)
            .WithName("GetAllReservations")
            .WithSummary("Get all reservations")
            .Produces<IReadOnlyList<ReservationResponse>>();

        group.MapGet("/{id:guid}", GetReservationById)
            .WithName("GetReservationById")
            .WithSummary("Get a reservation by ID")
            .Produces<ReservationResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapPatch("/{id:guid}/confirm", ConfirmReservation)
            .WithName("ConfirmReservation")
            .WithSummary("Confirm a reservation")
            .Produces<ReservationResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/{id:guid}/cancel", CancelReservation)
            .WithName("CancelReservation")
            .WithSummary("Cancel a reservation")
            .Produces<ReservationResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity);

        return app;
    }

    private static async Task<IResult> CreateReservation(
        CreateReservationRequest request,
        IReservationService service,
        IValidator<CreateReservationRequest> validator,
        CancellationToken ct)
    {
        var validation = await validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors);
        }

        var response = await service.CreateAsync(request, ct);
        return Results.CreatedAtRoute("GetReservationById", new { id = response.Id }, response);
    }

    private static async Task<IResult> GetAllReservations(
        IReservationService service,
        CancellationToken ct)
    {
        var reservations = await service.GetAllAsync(ct);
        return Results.Ok(reservations);
    }

    private static async Task<IResult> GetReservationById(
        Guid id,
        IReservationService service,
        CancellationToken ct)
    {
        var reservation = await service.GetByIdAsync(id, ct);
        return Results.Ok(reservation);
    }

    private static async Task<IResult> ConfirmReservation(
        Guid id,
        IReservationService service,
        CancellationToken ct)
    {
        var reservation = await service.ConfirmAsync(id, ct);
        return Results.Ok(reservation);
    }

    private static async Task<IResult> CancelReservation(
        Guid id,
        IReservationService service,
        CancellationToken ct)
    {
        var reservation = await service.CancelAsync(id, ct);
        return Results.Ok(reservation);
    }
}
