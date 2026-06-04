using EventApp.CustomExceptions;
using EventApp.Models;
using Xunit.v3.Priority;

namespace EventApp.Services
{
    [TestCaseOrderer(typeof(PriorityOrderer))]
    public class BookingServiceTests : IClassFixture<BookingServiceFixture>
    {
        private readonly BookingService _bookingService;
        public BookingServiceTests(BookingServiceFixture fixture)
        {
            _bookingService = fixture.bookingService;
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
            var eventId = 8;

            var newBooking = await _bookingService.CreateBookingAsync(eventId);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);

            Assert.Equal(eventId, expectedBookingWithPendingStatus.EventId);
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

    }
}
