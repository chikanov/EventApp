using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Application.Abstractions.Services;
using BookingService.Domain.CustomExceptions;
using BookingService.Domain.Entities;

namespace BookingService.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }
        public async Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            var newBooking = Booking.CreatePending(eventId, userId);

            await _bookingRepository.AddAsync(newBooking, cancellationToken);

            return newBooking;
        }
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
        {
            var book = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (book == null)
            {
                throw new NotFoundBookingException($"Booking with Id = {bookingId} does not exist.");
            }
            return book;
        }
        public async Task<Booking> UpdateBookingAsync(Booking book, CancellationToken cancellationToken = default)
        {
            var existBooking = await _bookingRepository.GetByIdAsync(book.Id, cancellationToken);

            if (existBooking == null)
            {
                throw new NotFoundBookingException($"Booking with Id = {book.Id} does not exist.");
            }

            if (existBooking != null)
            {
                existBooking.Id = book.Id;
                existBooking.EventId = book.EventId;
                existBooking.CreatedAt = book.CreatedAt;
                existBooking.ProcessedAt = book.ProcessedAt;
                existBooking.Status = book.Status;
            }
            await _bookingRepository.SaveChangesAsync(cancellationToken);
            return existBooking!;
        }

        public async Task<List<Booking>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetPendingAsync(cancellationToken);
        }

        public async Task<Booking> CancellationBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
        {
            var curBooking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            
            if (curBooking == null)
            {
                throw new NotFoundBookingException($"Booking with id - {bookingId} dose not exist.");
            }
            var bookingsCurUser = await _bookingRepository.GetUserOwnBookingAsync(userId, curBooking.EventId, cancellationToken);

            curBooking.Cancel();
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return curBooking;
        }
    }
}
