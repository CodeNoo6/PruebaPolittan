using Polittan.Reservations.Domain.Entities;

namespace Polittan.Reservations.Domain.Interfaces;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Reservation reservation, CancellationToken ct = default);
    Task<bool> ExistsDuplicateAsync(Reservation reservation, CancellationToken ct = default);
}
