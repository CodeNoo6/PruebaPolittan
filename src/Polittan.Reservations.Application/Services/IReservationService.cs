using Polittan.Reservations.Application.DTOs;

namespace Polittan.Reservations.Application.Services;

public interface IReservationService
{
    Task<ReservationResponse> CreateAsync(CreateReservationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken ct = default);
    Task<ReservationResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ReservationResponse> ConfirmAsync(Guid id, CancellationToken ct = default);
    Task<ReservationResponse> CancelAsync(Guid id, CancellationToken ct = default);
}
