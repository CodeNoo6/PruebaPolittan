using System.Collections.Concurrent;
using Polittan.Reservations.Domain.Entities;
using Polittan.Reservations.Domain.Interfaces;

namespace Polittan.Reservations.Infrastructure.Repositories;

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<Guid, Reservation> _store = new();

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(_store.TryGetValue(id, out var reservation) ? reservation : null);

    public Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Reservation>>([.. _store.Values.OrderByDescending(r => r.CreatedAt)]);

    public Task AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        _store[reservation.Id] = reservation;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsDuplicateAsync(Reservation candidate, CancellationToken ct = default)
    {
        var exists = _store.Values.Any(r =>
            string.Equals(r.CustomerName, candidate.CustomerName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Origin, candidate.Origin, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Destination, candidate.Destination, StringComparison.OrdinalIgnoreCase) &&
            r.Date == candidate.Date &&
            r.ServiceType == candidate.ServiceType);

        return Task.FromResult(exists);
    }
}
