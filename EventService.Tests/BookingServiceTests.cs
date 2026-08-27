using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Application.Abstractions.Services;
using EventService.Application.DTOs;
using EventService.Application.Services;
using EventService.Domain.CustomExceptions;
using EventService.Domain.Entities;
using EventService.Domain.Entities.Enum;
using EventService.Domain.Models.Enum;
using EventService.Infrastructure;
using EventService.Infrastructure.Persistence.DataAccess;
using EventService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Xunit.v3.Priority;

namespace EventApp.EventServiceTests
{
    [TestCaseOrderer(typeof(PriorityOrderer))]
    public class BookingServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly IUserRepository _userRepository;

        public BookingServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IEventService, EventService.Application.Services.EventService>();
            services.AddScoped<IBookingService, BookingService>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
            _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
            _userRepository = _scope.ServiceProvider.GetRequiredService<IUserRepository>();
        }
        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        private async Task<int> CreateTestEventAsync(int totalSeats = 15)
        {
            var futureDate = DateTime.UtcNow.AddDays(1);
            var created = await _eventService.CreateEventAsync(new CreateEventDto
            {
                Title = "Test Event",
                StartAt = futureDate,
                EndAt = futureDate.AddHours(2),
                TotalSeats = totalSeats
            });
            return created.Id;
        }

        [Fact, Priority(0)]
        public async Task CreateBookingWithExistEvent_ReturnBookingWithStatusPending()
        {
            await CreateEventsForTestsAsync();
            var ExistEventId = 3;
            var token = new CancellationToken();
            var statusPending = BookingStatus.Pending;
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId, userForTest.Id, token);

            Assert.Equal(statusPending, newBooking.Status);
        }

        [Fact, Priority(1)]
        public async Task CreateTwoBookingsOnOneEvent_ReturnDifferendId()
        {
            await CreateEventsForTestsAsync();
            var eventId = 4;
            var token = new CancellationToken();
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var firstBooking = await _bookingService.CreateBookingAsync(eventId, userForTest.Id, token);
            var secondBooking = await _bookingService.CreateBookingAsync(eventId, userForTest.Id, token);

            Assert.NotEqual(firstBooking.Id, secondBooking.Id);
        }

        [Fact, Priority(2)]
        public async Task GetBookingById_ReturnCorrectBooking()
        {
            await CreateEventsForTestsAsync();
            var eventId = 8;
            var token = new CancellationToken();
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var newBooking = await _bookingService.CreateBookingAsync(eventId, userForTest.Id, token);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id, userForTest.Id, token);

            Assert.Equal(eventId, expectedBookingWithPendingStatus?.EventId);
        }

        [Fact, Priority(3)]
        public async Task GetBookingById_ReturnCorrectStatus()
        {
            await CreateEventsForTestsAsync();
            var eventId = 9;
            var token = new CancellationToken();
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }
            var newBooking = await _bookingService.CreateBookingAsync(eventId, userForTest.Id, token);

            var expectedBookingWithPendingStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id, userForTest.Id, token);
            var pendingStatus = expectedBookingWithPendingStatus!.Status;
            expectedBookingWithPendingStatus.Status = BookingStatus.Confirmed;
            await _bookingService.UpdateBookingAsync(expectedBookingWithPendingStatus, token);
            var expectedBookingWithConfirmedStatus = await _bookingService.GetBookingByIdAsync(newBooking.Id, userForTest.Id, token);

            Assert.Equal(BookingStatus.Pending, pendingStatus);
            Assert.Equal(BookingStatus.Confirmed, expectedBookingWithConfirmedStatus!.Status);
        }

        [Fact, Priority(4)]
        public async Task CreateBookingWithNotExistEvent_ReturnNotFoundException()
        {
            var notExistEventId = -1;
            var expectedExceptionMessage = $"Event with Id = {notExistEventId} does not exist.";
            var token = new CancellationToken();
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistEventId, userForTest.Id, token));

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
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }
            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(deletedEventId, userForTest.Id, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(6)]
        public async Task GetBookingByNotExistId_ReturnNotFoundException()
        {
            var notExistId = Guid.NewGuid();
            var token = new CancellationToken();
            var expectedExceptionMessage = $"Booking with Id = {notExistId} does not exist.";
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _bookingService.GetBookingByIdAsync(notExistId, userForTest.Id, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(7)]
        public async Task CreateBooking_ReducesAvailableSeatsOn1()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedAvailableSeats = 99;
            var ExistEventId = 15;
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }
            var newBooking = await _bookingService.CreateBookingAsync(ExistEventId, userForTest.Id, token);

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
            var eventDto = ObjectMapperExtensions.MapEventToEventDto(expectedEvent);
            await _eventService.UpdateEventAsync(expectedEventId, eventDto, token);
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var firstBooking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
            var secondBooking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
            var thirdBooking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);

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
            var eventDto = ObjectMapperExtensions.MapEventToEventDto(expectedEvent);
            await _eventService.UpdateEventAsync(expectedEventId, eventDto, token);
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var firstBooking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);

            var exception = await Assert
            .ThrowsAsync<NoAvailableSeatsException>(async () => await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(10)]
        public async Task BookingNotExistingEvent_ReturnNotFoundException()
        {
            var token = new CancellationToken();
            var notExistingEventId = 29;
            var expectedExceptionMessage = $"Event with Id = {notExistingEventId} does not exist.";
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var exception = await Assert
            .ThrowsAsync<NotFoundException>(async () => await _bookingService.CreateBookingAsync(notExistingEventId, userForTest.Id, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(11)]
        public async Task SwitchingToConfirmation_ReturnStatusConfirmed()
        {
            await CreateEventsForTestsAsync();
            var token = new CancellationToken();
            var expectedEventId = 10;
            var expectedStatus = BookingStatus.Confirmed;
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
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
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
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
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
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
            var eventDto = ObjectMapperExtensions.MapEventToEventDto(expectedEvent);
            await _eventService.UpdateEventAsync(expectedEventId, eventDto, token);
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var booking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
            booking.Confirm();

            booking.Reject();
            expectedEvent.ReleaseSeats();

            var newBooking = await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);

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
            var eventDto = ObjectMapperExtensions.MapEventToEventDto(expectedEvent);
            await _eventService.UpdateEventAsync(expectedEventId, eventDto, token);
            var numbers = Enumerable.Range(0, 20).ToArray();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            await Parallel.ForEachAsync(numbers, options, async (numbers, token) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                try
                {
                    var newBooking = await bookingService.CreateBookingAsync(expectedEventId, userForTest.Id);
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

            using var scope = _serviceProvider.CreateScope();
            var eventsService = scope.ServiceProvider.GetRequiredService<IEventService>();

            var curEvent = await eventsService.GetByIdAsync(expectedEventId, token);

            Assert.Equal(expectedSaccesfullBooking, SaccesfullBookingCount);
            Assert.Equal(expectedNoAvailableSeatsExceptionCount, NoAvailableSeatsExceptionCount);
            Assert.Equal(expectedAvailableSeats, curEvent?.AvailableSeats);
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
            var eventDto = ObjectMapperExtensions.MapEventToEventDto(expectedEvent);
            await _eventService.UpdateEventAsync(expectedEventId, eventDto, token);
            var numbers = Enumerable.Range(0, 10).ToArray();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 5 };
            var cuncuretBookingIdsBag = new ConcurrentBag<Guid>();
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            await Parallel.ForEachAsync(numbers, options, async (numbers, token) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                try
                {
                    var newBooking = await bookingService.CreateBookingAsync(expectedEventId, userForTest.Id);
                    if (newBooking != null)
                    {
                        cuncuretBookingIdsBag.Add(newBooking.Id);
                    }
                }
                finally { }
            });
            var uniqueIdsCount = cuncuretBookingIdsBag.Distinct().Count();

            Assert.Equal(expectedNoAvailableSeatsExceptionCount, uniqueIdsCount);
        }
        [Fact, Priority(17)]
        public async Task TryBookPastEvent_Return_PastEventBookingException()
        {
            var token = new CancellationToken();
            var expectedExceptionMessage = "You cannot book an event that has already taken place.";
            var pastEvent = new CreateEventDto
            {
                Title = "Test Event",
                StartAt = DateTime.UtcNow.AddDays(-2),
                EndAt = DateTime.UtcNow.AddDays(-1),
                TotalSeats = 10
            };
            var expectedPastEvent = await _eventService.CreateEventAsync(pastEvent, token);
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }

            var exception = await Assert
            .ThrowsAsync<PastEventBookingException>(async () => await _bookingService.CreateBookingAsync(expectedPastEvent.Id, userForTest.Id, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(17)]
        public async Task WhenThelimitOfActiveBookIsReached_ANewBookIsNotCreated()
        {
            var token = new CancellationToken();
            var expectedExceptionMessage = "The limit of active armor has been reached.";
            var expectedBookingLimitCount = 10;
            int count = 0;
            var expectedEventId = await CreateTestEventAsync();
            var userForTest = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest == null)
            {
                userForTest = CreateUserForTest();
                await _userRepository.AddAsync(userForTest, token);
            }
            for (int i = 0; i < expectedBookingLimitCount; i++)
            {
                await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token);
            }
            var exception = await Assert
            .ThrowsAsync<ActiveLeasesExceededException>(async () => await _bookingService.CreateBookingAsync(expectedEventId, userForTest.Id, token));
            count++;
            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(18)]
        public async Task TheLimitsSetByDifferentUsersDoNotAffectEachOther()
        {
            var token = new CancellationToken();
            var expectedSuccessfulBookingCount = 11;
            int successfulBookingCount = 0;
            var expectedEventId = await CreateTestEventAsync();
            var userForTest1 = await _userRepository.GetByLogin("UserForTests", token);
            if (userForTest1 == null)
            {
                userForTest1 = CreateUserForTest();
                await _userRepository.AddAsync(userForTest1, token);
            }
            string hashPassword;
            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes("12345678");
                var hashBytes = sha256.ComputeHash(passwordBytes);

                hashPassword = Convert.ToHexString(hashBytes);
            }
            var userForTest2 = User.CreateUser(
                    "UserForTests2",
                    hashPassword,
                    UserRoles.Admin
                );
            await _userRepository.AddAsync(userForTest2, token);

            for (int i = 0; i < 5; i++)
            {
                await _bookingService.CreateBookingAsync(expectedEventId, userForTest1.Id, token);
                successfulBookingCount++;
            }
            for (int i = 0; i < 6; i++)
            {
                await _bookingService.CreateBookingAsync(expectedEventId, userForTest2.Id, token);
                successfulBookingCount++;
            }

            Assert.Equal(expectedSuccessfulBookingCount, successfulBookingCount);
        }

        public async Task CreateEventsForTestsAsync()
        {
            var token = new CancellationToken();
            var events = await _eventService.GetAllAsync(1, 15, null, null, null, token);
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
         /*   if (!events.ListEvents!.Any())
            {
                foreach (var @event in expectedEvents)
                {
                    await _eventService.CreateEventAsync(@event, token);
                }
            }
            else
            { */
                foreach (var ev in events.ListEvents!)
                {
                    await _eventService.DeleteEventAsync(ev.Id, token);
                }
                foreach (var @event in expectedEvents)
                {
                    await _eventService.CreateEventAsync(@event, token);
                }
         /*   } */
        }

        public User CreateUserForTest()
        {
            string hashPassword;
            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes("12345678");
                var hashBytes = sha256.ComputeHash(passwordBytes);

                hashPassword = Convert.ToHexString(hashBytes);
            }
            return User.CreateUser(
                    "UserForTests",
                    hashPassword,
                    UserRoles.Admin
                );
        }
    }
}
