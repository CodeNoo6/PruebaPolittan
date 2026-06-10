using Microsoft.EntityFrameworkCore;
using Polittan.Reservations.Domain.Entities;
using Polittan.Reservations.Domain.Interfaces;
using Polittan.Reservations.Infrastructure.Persistence;

namespace Polittan.Reservations.Infrastructure.Repositories;

public sealed class EfReservationRepository(ReservationsDbContext db) : IReservationRepository
{
    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Reservations.FindAsync([id], ct);

    public async Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default) =>
        await db.Reservations
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

    public async Task AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        db.Reservations.Add(reservation);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsDuplicateAsync(Reservation candidate, CancellationToken ct = default) =>
        await db.Reservations.AnyAsync(r =>
            r.CustomerName.ToLower() == candidate.CustomerName.ToLower() &&
            r.Origin.ToLower() == candidate.Origin.ToLower() &&
            r.Destination.ToLower() == candidate.Destination.ToLower() &&
            r.Date == candidate.Date &&
            r.ServiceType == candidate.ServiceType,
            ct);
}
