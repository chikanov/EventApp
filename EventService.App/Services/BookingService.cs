using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;

namespace EventApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly IEventService _eventService;
        public BookingService(IEventService eventService)
        {
            _eventService = eventService;
        }
        public static List<Booking> _bookings = new()
        {
            new Booking() {Id = Guid.NewGuid(), 
                           EventId = 1, 
                           CreatedAt = DateTime.Now, 
                           ProcessedAt = DateTime.Now.AddMinutes(5), 
                           Status = Booking.BookingStatus.Confirmed.ToString() },
            new Booking() {Id = Guid.NewGuid(),
                           EventId = 2,
                           CreatedAt = DateTime.Now,
                           ProcessedAt = DateTime.Now.AddMinutes(5),
                           Status = Booking.BookingStatus.Rejected.ToString() },
            new Booking() {Id = Guid.NewGuid(),
                           EventId = 3,
                           CreatedAt = DateTime.Now,
                           ProcessedAt = DateTime.Now.AddMinutes(5),
                           Status = Booking.BookingStatus.Pending.ToString() },
        };
        public async Task<Booking> CreateBookingAsync(int eventId)
        {
            if (_eventService.GetById(eventId) == null)
            {
                throw new NotFoundException($"Event with Id = {eventId} does not exist.");
            }
            var newBooking = new Booking()
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                CreatedAt = DateTime.Now,
                Status = Booking.BookingStatus.Pending.ToString(),
            };
            _bookings.Add(newBooking);

            return newBooking;
        }
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            if (!_bookings.Any(b => b.Id == bookingId))
            {
                throw new NotFoundException($"Booking with Id = {bookingId} does not exist.");
            }
            return _bookings.FirstOrDefault(b => b.Id == bookingId);
        }
    }
}
