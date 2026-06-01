using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.Const;

namespace EventApp.Services
{
    public class BookingService : IBookingService
    {
        public readonly IEventService _eventService;
        public readonly ILogger _logger;
        public BookingService(IEventService eventService, ILogger logger)
        {
            _eventService = eventService;
            _logger = logger;
        }
        public static List<Booking> _bookings = new()
        {
            new Booking() {Id = new Guid(), 
                           EventId = 1, 
                           CreatedAt = DateTime.Now, 
                           ProcessedAt = DateTime.Now.AddMinutes(5), 
                           Status = BookingStatus.Confirmed },
            new Booking() {Id = new Guid(),
                           EventId = 2,
                           CreatedAt = DateTime.Now,
                           ProcessedAt = DateTime.Now.AddMinutes(5),
                           Status = BookingStatus.Rejected },
            new Booking() {Id = new Guid(),
                           EventId = 3,
                           CreatedAt = DateTime.Now,
                           ProcessedAt = DateTime.Now.AddMinutes(5),
                           Status = BookingStatus.Pending },
        };
        public async Task<Booking> CreateBookingAsync(int eventId)
        {
            await Task.Delay(2000);
            var newBooking = new Booking()
            {
                Id = new Guid(),
                EventId = eventId,
                CreatedAt = new DateTime(),
                Status = BookingStatus.Pending,
            };
            _bookings.Add(newBooking);

            return newBooking;
        }

        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            await Task.Delay(2000);
            return _bookings.FirstOrDefault(b => b.Id == bookingId);
        }
    }
}
