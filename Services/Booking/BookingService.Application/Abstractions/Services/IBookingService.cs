using BookingService.Domain.Entities;

namespace BookingService.Application.Abstractions.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken cancellationToken = default);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<Booking>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task<Booking> UpdateBookingAsync(Booking book, CancellationToken cancellationToken = default);
        Task<Booking> CancellationBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default);
    }
}
