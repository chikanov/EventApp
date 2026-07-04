using EventApp.CustomExceptions;
using EventApp.DataAccess;
using EventApp.Interfaces;
using EventApp.Models.DTO;
using EventApp.Models.Models.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using Xunit.v3.Priority;

namespace EventApp.Services
{
    [TestCaseOrderer(typeof(PriorityOrderer))]
    public class BookingServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        }
        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        [Fact, Priority(0)]
        public async Task CreateBookingWithExistEvent_ReturnBookingWithStatusPending()
        {
            await CreateEventsForTestsAsync();
            var ExistEventId = 3;
            var token = new CancellationToken();
            var statusPending = BookingStatus.Pending;

            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId, token);

            Assert.Equal(statusPending, newBooking.Status);
        }

        [Fact, Priority(1)]
        public async Task CreateTwoBookingsOnOneEvent_ReturnDifferendId()
        {
            await CreateEventsForTestsAsync();
            var eventId = 4;
            var token = new CancellationToken();

            var firstBooking = await _bookingService.CreateBookingAsync(eventId, token);
            var secondBooking = await _bookingService.CreateBookingAsync(eventId, token);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact, Priority(2)]
        public async Task GetBookingById_ReturnCorrectBooking()
        {
            await CreateEventsForTestsAsync();
            var eventId = 8;
            var token = new CancellationToken();

            var newBooking = await _bookingService.CreateBookingAsync(eventId, token);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id, token);

            Assert.Equal(eventId, expectedBookingWithPendingStatus?.EventId);
        }

        [Fact, Priority(3)]
        public async Task GetBookingById_ReturnCorrectStatus()
        {
            await CreateEventsForTestsAsync();
            var eventId = 9;
            var token = new CancellationToken();
            var newBooking = await _bookingService.CreateBookingAsync(eventId, token);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id, token);
            var pendingStatus = expectedBookingWithPendingStatus!.Status;
            expectedBookingWithPendingStatus.Status = BookingStatus.Confirmed;
            await _bookingService.UpdateBookingAsync(expectedBookingWithPendingStatus, token);
            var expectedBookingWithConfirmedStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id, token);

            Assert.Equal(BookingStatus.Pending, pendingStatus);
            Assert.Equal(BookingStatus.Confirmed, expectedBookingWithConfirmedStatus!.Status);
        }

        [Fact, Priority(4)]
        public async Task CreateBookingWithNotExistEvent_ReturnNotFoundException()
        {
            var notExistEventId = -1;
            var expectedExceptionMessage = $"Event with Id = {notExistEventId} does not exist.";
            var token = new CancellationToken();

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistEventId, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(5)]
        public async Task CreateBookingWithDeletedEvent_ReturnNotFoundException()
        {
            await CreateEventsForTestsAsync();
            var deletedEventId = 1;
            var token = new CancellationToken();
            var expectedExceptionMessage = $"Event with Id = {deletedEventId} does not exist.";
            await _eventService.DeleteEventAsync(deletedEventId, token);
            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(deletedEventId, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(6)]
        public async Task GetBookingByNotExistId_ReturnNotFoundException()
        {
            var notExistId = Guid.NewGuid();
            var token = new CancellationToken();
            var expectedExceptionMessage = $"Booking with Id = {notExistId} does not exist.";

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.GetBookingByIdAsync(notExistId, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(7)]
        public async Task CreateBooking_ReducesAvailableSeatsOn1()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedAvailableSeats = 99;
            var ExistEventId = 15;
            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId, token);

            var currentEvent = await _eventService.GetByIdAsync(newBooking.EventId, token);

            Assert.Equal(expectedAvailableSeats, currentEvent!.AvailableSeats);
        }

        [Fact, Priority(8)]
        public async Task CreatingMultipleBookingsByLimit_AllSuccessUniqueId()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 14;
            var expectedTotalSeats = 3;
            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);
            expectedEvent!.TotalSeats = expectedTotalSeats;
            expectedEvent.AvailableSeats = expectedTotalSeats;
            await _eventService.UpdateEventAsync(expectedEventId, expectedEvent, token);

            var firstBooking = await _bookingService.CreateBookingAsync(expectedEventId, token);
            var secondBooking = await _bookingService.CreateBookingAsync(expectedEventId, token);
            var thirdBooking = await _bookingService.CreateBookingAsync(expectedEventId, token);

            Assert.NotNull(firstBooking);
            Assert.NotNull(secondBooking);
            Assert.NotNull(thirdBooking);
            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
            Assert.NotEqual(secondBooking.Id, thirdBooking.Id);
        }

        [Fact, Priority(9)]
        public async Task CreatingMultipleBookingsMoreThanLimit_ReturnNoAvailableSeatsException()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 13;
            var expectedTotalSeats = 1;
            var expectedExceptionMessage = "No available seats for this event.";
            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);
            expectedEvent!.TotalSeats = expectedTotalSeats;
            expectedEvent.AvailableSeats = expectedTotalSeats;
            await _eventService.UpdateEventAsync(expectedEventId, expectedEvent, token);

            var firstBooking = await _bookingService.CreateBookingAsync(expectedEventId, token);

            var exception = await Assert
            .ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(expectedEventId, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(10)]
        public async Task BookingNotExistingEvent_ReturnNotFoundException()
        {
            var token = new CancellationToken();
            var notExistingEventId = 29;
            var expectedExceptionMessage = $"Event with Id = {notExistingEventId} does not exist.";

            var exception = await Assert
            .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistingEventId, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(11)]
        public async Task SwitchingToConfirmation_ReturnStatusConfirmed()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 10;
            var expectedStatus = BookingStatus.Confirmed;

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, token);
            booking.Confirm();

            Assert.Equal(expectedStatus, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact, Priority(12)]
        public async Task SwitchingToRejection_ReturnStatusRejected()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 9;
            var expectedStatus = BookingStatus.Rejected;
            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, token);
            booking.Confirm();

            booking.Reject();
            expectedEvent!.ReleaseSeats();

            Assert.Equal(expectedStatus, booking.Status);
            Assert.NotNull(booking.ProcessedAt);
        }

        [Fact, Priority(13)]
        public async Task SwitchingToRejection_AvailableSeatsRecovering()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 7;
            var expectedAvailableSeats = 100;
            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, token);
            booking.Confirm();

            booking.Reject();
            expectedEvent!.ReleaseSeats();

            Assert.Equal(expectedAvailableSeats, expectedEvent.AvailableSeats);
        }

        [Fact, Priority(14)]
        public async Task SwitchingToRejection_CanSuccessfullyCreateNewBooking()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 6;
            var expectedSeats = 1;
            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);
            expectedEvent!.TotalSeats = expectedSeats;
            expectedEvent.AvailableSeats = expectedSeats;
            await _eventService.UpdateEventAsync(expectedEventId, expectedEvent, token);

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, token);
            booking.Confirm();

            booking.Reject();
            expectedEvent.ReleaseSeats();

            var newBooking = await _bookingService.CreateBookingAsync(expectedEventId, token);

            Assert.NotNull(newBooking);
        }
        [Fact, Priority(15)]
        public async Task OverbookingTest_ReturnFiveSuccesfullBooking15NoAvailableSeatsException0AvailableSeats()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedSaccesfullBooking = 5;
            var expectedNoAvailableSeatsExceptionCount = 15;
            var expectedAvailableSeats = 0;
            var expectedEventId = 6;
            var SaccesfullBookingCount = 0;
            var NoAvailableSeatsExceptionCount = 0;

            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);
            expectedEvent!.TotalSeats = expectedSaccesfullBooking;
            expectedEvent.AvailableSeats = expectedSaccesfullBooking;
            await _eventService.UpdateEventAsync(expectedEventId, expectedEvent, token);
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
                catch (NoAvailableSeatsException)
                {
                    Interlocked.Increment(ref NoAvailableSeatsExceptionCount);
                }
            });

            Assert.Equal(expectedSaccesfullBooking, SaccesfullBookingCount);
            Assert.Equal(expectedNoAvailableSeatsExceptionCount, NoAvailableSeatsExceptionCount);
            Assert.Equal(expectedAvailableSeats, expectedEvent.AvailableSeats);
        }

        [Fact, Priority(16)]
        public async Task IdUniquenessTest_Return10UniqueBookingId()
        {
            await CreateEventsForTestsAsync();
            var expectedSaccesfullBooking = 10;
            var expectedNoAvailableSeatsExceptionCount = 10;
            var expectedEventId = 6;
            var token = new CancellationToken();

            var expectedEvent = await _eventService.GetByIdAsync(expectedEventId, token);
            expectedEvent!.TotalSeats = expectedSaccesfullBooking;
            expectedEvent.AvailableSeats = expectedSaccesfullBooking;
            await _eventService.UpdateEventAsync(expectedEventId, expectedEvent, token);
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

        public async Task CreateEventsForTestsAsync()
        {
            var token = new CancellationToken();
            var events = await _eventService.GetAllAsync(1,15,null, null, null, token);
            var expectedEvents = new List<CreateEventDto>()
                {
                    new CreateEventDto(){ Title = "Title1", Description = "Description1", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title2", Description = "Description2", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title3", Description = "Description3", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title4", Description = "Description4", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title5", Description = "Description5", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title6", Description = "Description6", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title7", Description = "Description7", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title8", Description = "Description8", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title9", Description = "Description9", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title10", Description = "Description10", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title11", Description = "Description11", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title12", Description = "Description12", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title13", Description = "Description13", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title14", Description = "Description14", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9), TotalSeats = 100},
                    new CreateEventDto(){ Title = "Title15", Description = "Description15", StartAt = DateTime.Now.AddDays(9), EndAt = DateTime.Now.AddDays(10), TotalSeats = 100}
                };
            if (!events.ListEvents.Any())
            {
                foreach (var @event in expectedEvents)
                {
                    await _eventService.CreateEventAsync(@event, token);
                }
            }
            else
            {
                foreach (var ev in events.ListEvents)
                {
                    await _eventService.DeleteEventAsync(ev.Id, token);
                }
                foreach (var @event in expectedEvents)
                {
                    await _eventService.CreateEventAsync(@event, token);
                }
            }
        }
    }
}
