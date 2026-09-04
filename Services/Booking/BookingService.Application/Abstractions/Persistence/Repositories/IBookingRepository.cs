using BookingService.Domain.Entities;

namespace BookingService.Application.Abstractions.Persistence.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<Booking>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Booking book, CancellationToken ct = default);
        Task<List<Booking>> GetPendingAsync(CancellationToken ct = default);
        Task<bool> AnyAsync(CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<List<Booking>> GetUserOwnBookingAsync(Guid userId, int eventId, CancellationToken ct = default);
    }
}
