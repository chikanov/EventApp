using EventApp.CustomExceptions;
using EventApp.DataAccess;
using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Services
{
    public class BookingService : IBookingService
    {
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly AppDbContext _context;
        public BookingService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Booking> CreateBookingAsync(int eventId, CancellationToken cancellationToken = default)
        {
            await _processingSemaphore.WaitAsync(cancellationToken);
            try
            {
                var currentEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
                if (currentEvent == null)
                {
                    throw new NotFoundException($"Event with Id = {eventId} does not exist.");
                }

                if (!currentEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException();
                else
                {
                    var newBooking = Booking.CreatePending(eventId);


                    await _context.Bookings.AddAsync(newBooking, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    return newBooking;
                }
            }
            finally { _processingSemaphore.Release(); }
        }
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            if (!await _context.Bookings.AnyAsync(b => b.Id == bookingId, cancellationToken))
            {
                throw new NotFoundException($"Booking with Id = {bookingId} does not exist.");
            }
            return await _context.Bookings.AsNoTrackingWithIdentityResolution().Include(b => b.Event).FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);
        }
        public async Task<Booking> UpdateBookingAsync(Booking book, CancellationToken cancellationToken = default)
        {
            var existBooking = await _context.Bookings.FirstOrDefaultAsync(e => e.Id == book.Id, cancellationToken);

            if (existBooking == null)
            {
                throw new NotFoundException($"Booking with Id = {book.Id} does not exist.");
            }

            if (existBooking != null)
            {
                existBooking.Id = book.Id;
                existBooking.EventId = book.EventId;
                existBooking.CreatedAt = book.CreatedAt;
                existBooking.ProcessedAt = book.ProcessedAt;
                existBooking.Status = book.Status;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return existBooking;
        }

        public async Task<List<Booking>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .ToListAsync(cancellationToken);
        }
    }
}
