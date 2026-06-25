using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace EventApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly IEventService _eventService;
        private readonly object _bookingLock = new();
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
            lock (_bookingLock)
            {
                var currentEvent = _eventService.GetById(eventId);
                if (currentEvent == null)
                {
                    throw new NotFoundException($"Event with Id = {eventId} does not exist.");
                }

                if (!currentEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException();
                else
                {
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
            }
        }
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId)
        {
            if (!_bookings.Any(b => b.Id == bookingId))
            {
                throw new NotFoundException($"Booking with Id = {bookingId} does not exist.");
            }
            return _bookings.FirstOrDefault(b => b.Id == bookingId);
        }
        public Booking Update(Booking book)
        {
            var existBooking = _bookings.FirstOrDefault(e => e.Id == book.Id);

            if (existBooking == null)
            {
                throw new NotFoundException($"Booking with Id = {book.Id} does not exist.");
            }

            if (existBooking != null)
            {
                existBooking.Id = book.Id;
                existBooking.EventId = book.EventId;
                existBooking.Status = book.Status;
                existBooking.CreatedAt = book.CreatedAt;
                existBooking.ProcessedAt = book.ProcessedAt;
            }
            return existBooking;
        }

        public IEnumerable<Booking> GetPending()
        {
            return _bookings.Where(b => b.Status == Booking.BookingStatus.Pending.ToString());
        }
    }
}
