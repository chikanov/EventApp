using EventApp.BackgroundServices;
using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Xunit.v3.Priority;
using static EventApp.Models.Booking;

namespace EventApp.Services
{
    [TestCaseOrderer(typeof(PriorityOrderer))]
    public class BookingServiceTests : IClassFixture<BookingServiceFixture>
    {
        private readonly BookingService _bookingService;
        private readonly IEventService _eventService;
        public BookingServiceTests(BookingServiceFixture fixture)
        {
            _bookingService = fixture.bookingService;
            _eventService = fixture._eventService;
        }

        [Fact, Priority(0)]
        public async Task CreateBookingWithExistEvent_ReturnBookingWithStatusPending()
        {
            var ExistEventId = 3;
            var statusPending = Booking.BookingStatus.Pending.ToString();

            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId);

            Assert.Equal(statusPending, newBooking.Status);
        }

        [Fact, Priority(1)]
        public async Task CreateTwoBookingsOnOneEvent_ReturnDifferendId()
        {
            var eventId = 4;

            var firstBooking = await _bookingService.CreateBookingAsync(eventId);
            var secondBooking = await _bookingService.CreateBookingAsync(eventId);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact, Priority(2)]
        public async Task GetBookingById_ReturnCorrectBooking()
        {
            var eventId = 8;

            var newBooking = await _bookingService.CreateBookingAsync(eventId);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);

            Assert.Equal(eventId, expectedBookingWithPendingStatus?.EventId);
        }

        [Fact, Priority(3)]
        public async Task GetBookingById_ReturnCorrectStatus()
        {
            var eventId = 9;
            var newBooking = await _bookingService.CreateBookingAsync(eventId);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);
            var pendingStatus = expectedBookingWithPendingStatus.Status;
            expectedBookingWithPendingStatus.Status = Booking.BookingStatus.Confirmed.ToString();
            var expectedBookingWithConfirmedStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);

            Assert.Equal(Booking.BookingStatus.Pending.ToString(), pendingStatus);
            Assert.Equal(Booking.BookingStatus.Confirmed.ToString(), expectedBookingWithConfirmedStatus.Status);
        }

        [Fact, Priority(4)]
        public async Task CreateBookingWithNotExistEvent_ReturnNotFoundException()
        {
            var notExistEventId = -1;
            var expectedExceptionMessage = $"Event with Id = {notExistEventId} does not exist.";

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistEventId));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(5)]
        public async Task CreateBookingWithDeletedEvent_ReturnNotFoundException()
        {
            var deletedEventId = 1;
            var expectedExceptionMessage = $"Event with Id = {deletedEventId} does not exist.";
            var eventService = new EventService();
            eventService.Delete(deletedEventId);
            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(deletedEventId));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(6)]
        public async Task GetBookingByNotExistId_ReturnNotFoundException()
        {
            var notExistId = Guid.NewGuid();
            var expectedExceptionMessage = $"Booking with Id = {notExistId} does not exist.";

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.GetBookingByIdAsync(notExistId));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(7)]
        public async Task CreateBooking_ReducesAvailableSeatsOn1()
        {
            var expectedAvailableSeats = 99;
            var ExistEventId = 15;
            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId);

            var currentEvent = _eventService.GetById(newBooking.EventId);

            Assert.Equal(expectedAvailableSeats, currentEvent.AvailableSeats);
        }

        [Fact, Priority(8)]
        public async Task CreatingMultipleBookingsByLimit_AllSuccessUniqueId()
        {
            var expectedEventId = 14;
            var expectedTotalSeats = 3;
            var expectedEvent = _eventService.GetById(expectedEventId);
            expectedEvent.TotalSeats = expectedTotalSeats;
            expectedEvent.AvailableSeats = expectedTotalSeats;
            _eventService.Update(expectedEventId, expectedEvent);

            var firstBooking = await _bookingService.CreateBookingAsync(expectedEventId);
            var secondBooking = await _bookingService.CreateBookingAsync(expectedEventId);
            var thirdBooking = await _bookingService.CreateBookingAsync(expectedEventId);

            Assert.NotNull(firstBooking);
            Assert.NotNull(secondBooking);
            Assert.NotNull(thirdBooking);
            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
            Assert.NotEqual(secondBooking.Id, thirdBooking.Id);
        }

        [Fact, Priority(9)]
        public async Task CreatingMultipleBookingsMoreThanLimit_ReturnNoAvailableSeatsException()
        {
            var expectedEventId = 13;
            var expectedTotalSeats = 1;
            var expectedExceptionMessage = "No available seats for this event.";
            var expectedEvent = _eventService.GetById(expectedEventId);
            expectedEvent.TotalSeats = expectedTotalSeats;
            expectedEvent.AvailableSeats = expectedTotalSeats;
            _eventService.Update(expectedEventId, expectedEvent);

            var firstBooking = await _bookingService.CreateBookingAsync(expectedEventId);

            var exception = await Assert
            .ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(expectedEventId));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(10)]
        public async Task BookingNotExistingEvent_ReturnNotFoundException()
        {
            var notExistingEventId = 29;
            var expectedExceptionMessage = $"Event with Id = {notExistingEventId} does not exist.";

            var exception = await Assert
            .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistingEventId));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(11)]
        public async Task SwitchingToConfirmation_ReturnStatusConfirmed()
        {
            var expectedEventId = 10;
            var expectedStatus = Booking.BookingStatus.Confirmed.ToString();

            var booking = await _bookingService.CreateBookingAsync(expectedEventId);
            booking.Confirm();

            Assert.Equal(expectedStatus, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact, Priority(12)]
        public async Task SwitchingToRejection_ReturnStatusRejected()
        {
            var expectedEventId = 9;
            var expectedStatus = Booking.BookingStatus.Rejected.ToString();
            var expectedEvent = _eventService.GetById(expectedEventId);

            var booking = await _bookingService.CreateBookingAsync(expectedEventId);
            booking.Confirm();

            booking.Reject();
            expectedEvent.ReleaseSeats();

            Assert.Equal(expectedStatus, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact, Priority(13)]
        public async Task SwitchingToRejection_AvailableSeatsRecovering()
        {
            var expectedEventId = 7;
            var expectedAvailableSeats = 100;
            var expectedStatus = Booking.BookingStatus.Rejected.ToString();
            var expectedEvent = _eventService.GetById(expectedEventId);

            var booking = await _bookingService.CreateBookingAsync(expectedEventId);
            booking.Confirm();

            booking.Reject();
            expectedEvent.ReleaseSeats();

            Assert.Equal(expectedAvailableSeats, expectedEvent.AvailableSeats);
        }

        [Fact, Priority(14)]
        public async Task SwitchingToRejection_CanSuccessfullyCreateNewBooking()
        {
            var expectedEventId = 6;
            var expectedSeats = 1;
            var expectedStatus = Booking.BookingStatus.Rejected.ToString();
            var expectedEvent = _eventService.GetById(expectedEventId);
            expectedEvent.TotalSeats = expectedSeats;
            expectedEvent.AvailableSeats = expectedSeats;
            _eventService.Update(expectedEventId, expectedEvent);

            var booking = await _bookingService.CreateBookingAsync(expectedEventId);
            booking.Confirm();

            booking.Reject();
            expectedEvent.ReleaseSeats();

            var newBooking = await _bookingService.CreateBookingAsync(expectedEventId);

            Assert.NotNull(newBooking);
        }
        [Fact, Priority(15)]
        public async Task OverbookingTest_ReturnFiveSuccesfullBooking15NoAvailableSeatsException0AvailableSeats()
        {
            var expectedSaccesfullBooking = 5;
            var expectedNoAvailableSeatsExceptionCount = 15;
            var expectedAvailableSeats = 0;
            var expectedEventId = 6;
            var SaccesfullBookingCount = 0;
            var NoAvailableSeatsExceptionCount = 0;

            var expectedEvent = _eventService.GetById(expectedEventId);
            expectedEvent.TotalSeats = expectedSaccesfullBooking;
            expectedEvent.AvailableSeats = expectedSaccesfullBooking;
            _eventService.Update(expectedEventId, expectedEvent);
            var token = new CancellationToken();
            var numbers = Enumerable.Range(0, 20).ToArray();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };

            await Parallel.ForEachAsync(numbers, options, async (numbers, token) =>
            {
                try
                {
                    var newBooking = await _bookingService.CreateBookingAsync(expectedEventId);
                    if (newBooking != null)
                    {
                        Interlocked.Increment(ref SaccesfullBookingCount);
                    }
                }
                catch (NoAvailableSeatsException ex)
                {
                    Interlocked.Increment(ref NoAvailableSeatsExceptionCount);
                }
            });

            Assert.Equal(expectedSaccesfullBooking, SaccesfullBookingCount);
            Assert.Equal(expectedNoAvailableSeatsExceptionCount, NoAvailableSeatsExceptionCount);
        }

        [Fact, Priority(16)]
        public async Task IdUniquenessTest_Return10UniqueBookingId()
        {
            var expectedSaccesfullBooking = 10;
            var expectedNoAvailableSeatsExceptionCount = 10;
            var expectedAvailableSeats = 0;
            var expectedEventId = 6;
            var SaccesfullBookingCount = 0;
            var NoAvailableSeatsExceptionCount = 0;

            var expectedEvent = _eventService.GetById(expectedEventId);
            expectedEvent.TotalSeats = expectedSaccesfullBooking;
            expectedEvent.AvailableSeats = expectedSaccesfullBooking;
            _eventService.Update(expectedEventId, expectedEvent);
            var token = new CancellationToken();
            var numbers = Enumerable.Range(0, 10).ToArray();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
            var cuncuretBookingIdsBag = new ConcurrentBag<Guid>();

            await Parallel.ForEachAsync(numbers, options, async (numbers, token) =>
            {
                try
                {
                    var newBooking = await _bookingService.CreateBookingAsync(expectedEventId);
                    if (newBooking != null)
                    {
                        cuncuretBookingIdsBag.Add(newBooking.Id);
                    }
                }
                finally {  }
            });
            var uniqueIdsCount = cuncuretBookingIdsBag.Distinct().Count();

            Assert.Equal(expectedNoAvailableSeatsExceptionCount, uniqueIdsCount);
        }
    }
}
