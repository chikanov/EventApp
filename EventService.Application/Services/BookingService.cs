using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Application.Abstractions.Services;
using EventService.Domain.CustomExceptions;
using EventService.Domain.Entities;
using EventService.Domain.Entities.Enum;

namespace EventService.Application.Services
{
    public class BookingService : IBookingService
    {
        private static readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private readonly IBookingRepository _bookingRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        public BookingService(IBookingRepository bookingRepository, IEventRepository eventRepository, IUserRepository userRepository)
        {
            _bookingRepository = bookingRepository;
            _eventRepository = eventRepository;
            _userRepository = userRepository;
        }
        public async Task<Booking> CreateBookingAsync(int eventId, Guid userId, CancellationToken cancellationToken = default)
        {
            await _processingSemaphore.WaitAsync(cancellationToken);
            try
            {
                var currentEvent = await _eventRepository.GetByIdAsync(eventId, cancellationToken);
                var currentUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (currentEvent == null)
                {
                    throw new NotFoundException($"Event with Id = {eventId} does not exist.");
                }
                if (currentUser == null)
                {
                    throw new NotFoundException($"User with Id = {userId} does not exist.");
                }
                if (currentEvent.EndAt < DateTime.UtcNow)
                {
                    throw new PastEventBookingException("You cannot book an event that has already taken place.");
                }
                if (currentUser!.Bookings != null && currentUser!.Bookings.Count == 10)
                {
                    throw new ActiveLeasesExceededException("The limit of active armor has been reached.");
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
        public async Task<Booking?> GetBookingByIdAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
        {
            var book = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            var curUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (book == null)
            {
                throw new NotFoundException($"Booking with Id = {bookingId} does not exist.");
            }
            if (curUser == null)
            {
                throw new NotFoundException($"User with Id = {userId} does not exist.");
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

        public async Task<Booking> CancellationBookingAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken = default)
        {
            var curUser = await _userRepository.GetByIdAsync(userId, cancellationToken);
            var curBooking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);

            if (curUser == null) 
            {
                throw new NotFoundException($"User with id - {userId} dose not exist.");
            }
            if (curBooking == null)
            {
                throw new NotFoundException($"Booking with id - {bookingId} dose not exist.");
            }
            if (curUser.Role == UserRoles.User && !curUser.Bookings.Select(b => b.Id).Contains(bookingId))
            {
                throw new PermissionDeniedException("The user does not have the rights to perform this operation.");
            }

            curBooking.Cancel();
            await _bookingRepository.SaveChangesAsync(cancellationToken);

            return curBooking;
        }
    }
}
