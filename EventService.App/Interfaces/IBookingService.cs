using EventApp.Models;

namespace EventApp.Interfaces
{
    public interface IBookingService
    {
        Task<Booking> CreateBookingAsync(int eventId);
        Task<Booking?> GetBookingByIdAsync(Guid bookingId);
    }
}
