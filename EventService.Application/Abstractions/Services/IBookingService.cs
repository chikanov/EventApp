using EventService.Domain.Entities;

namespace EventService.Application.Abstractions.Services
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken cancellationToken = default);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
        Task<List<Booking>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task<Booking> UpdateBookingAsync(Booking book, CancellationToken cancellationToken = default);
    }
}
