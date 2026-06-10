using Microsoft.EntityFrameworkCore;
using Polittan.Reservations.API.Endpoints;
using Polittan.Reservations.API.Middleware;
using Polittan.Reservations.Application;
using Polittan.Reservations.Infrastructure;
using Polittan.Reservations.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

// Crea la BD y el esquema si no existen
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReservationsDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "Polittan Reservations API";
    options.Theme = ScalarTheme.Purple;
});

app.MapReservationEndpoints();

app.Run();
