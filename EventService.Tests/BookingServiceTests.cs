using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.Const;
using EventApp.Services;
using Microsoft.Extensions.Logging;

namespace EventApp
{
    public class BookingServiceTests : IClassFixture<BookingServiceFixture>
    {
        private readonly BookingService _bookingService;
        public BookingServiceTests(BookingServiceFixture fixture)
        {
            _bookingService = fixture.bookingService;
        }

        [Fact]
        public async Task CreateBookingWithExistEvent_ReturnBookingWithStatusPending()
        {
            var ExistEventId = 3;
            var statusPending = BookingStatus.Pending;

            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId);

            Assert.Equal(statusPending, newBooking.Status);
        }

        [Fact]
        public async Task CreateTwoBookingsOnOneEvent_ReturnDifferendId()
        {
            var eventId = 4;

            var firstBooking = await _bookingService.CreateBookingAsync(eventId);
            var secondBooking = await _bookingService.CreateBookingAsync(eventId);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact]
        public async Task GetBookingById_ReturnCorrectBooking()
        {
            var eventId = 8;

            var newBooking = await _bookingService.CreateBookingAsync(eventId);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);

            Assert.Equal(eventId, expectedBookingWithPendingStatus.EventId);
        }

        [Fact]
        public async Task GetBookingById_ReturnCorrectStatus()
        {
            var eventId = 9;
            var newBooking = await _bookingService.CreateBookingAsync(eventId);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);
            var pendingStatus = expectedBookingWithPendingStatus.Status;
            expectedBookingWithPendingStatus.Status = BookingStatus.Confirmed;
            var expectedBookingWithConfirmedStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id);

            Assert.Equal(BookingStatus.Pending, pendingStatus);
            Assert.Equal(BookingStatus.Confirmed, expectedBookingWithConfirmedStatus.Status);
        }

        [Fact]
        public async Task CretaeBookingWithNotExistEvent_ReturnNotFoundException()
        {
            var notExistEventId = -1;
            var expectedExceptionMessage = $"Event with Id = {notExistEventId} does not exist.";

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistEventId));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact]
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
