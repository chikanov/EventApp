using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Application.Abstractions.Services;
using EventService.Domain.CustomExceptions;
using EventService.Domain.Entities;

namespace EventService.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
        }
        public async Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            await _processingSemaphore.WaitAsync(cancellationToken);
            try
            {
                var currentEvent = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
                if (currentEvent == null)
                {
                    throw new NotFoundException($"Event with Id = {eventId} does not exist.");
                }

                if (!currentEvent.TryReserveSeats())
                    throw new NoAvailableSeatsException();
                else
                {
                    var newBooking = Booking.CreatePending(eventId, userId);

                    await _bookingRepository.AddAsync(newBooking, cancellationToken);

                    return newBooking;
                }
            }
            finally { _processingSemaphore.Release(); }
        }
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
        {
            var book = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (book == null)
            {
                throw new NotFoundException($"Booking with Id = {bookingId} does not exist.");
            }
            return book;
        }
        public async Task<Booking> UpdateBookingAsync(Booking book, CancellationToken cancellationToken = default)
        {
            var existBooking = await _bookingRepository.GetByIdAsync(book.Id, cancellationToken);

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
            await _bookingRepository.SaveChangesAsync(cancellationToken);
            return existBooking!;
        }

        public async Task<List<Booking>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.GetPendingAsync(cancellationToken);
        }
    }
}
