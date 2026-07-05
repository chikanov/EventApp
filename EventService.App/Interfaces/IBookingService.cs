using EventApp.Models;

namespace EventApp.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(int eventId, CancellationToken cancellationToken = default);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
        Task<List<Booking>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task<Booking> UpdateBookingAsync(Booking book, CancellationToken cancellationToken = default);
    }
}
