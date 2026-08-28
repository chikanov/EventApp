using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Domain.Entities;
using EventService.Domain.Entities.Enum;
using EventService.Domain.Models.Enum;
using EventService.Infrastructure.Persistence.DataAccess;
using EventService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Testcontainers.PostgreSql;

namespace EventApp.EventServiceIntegrationTests
{
    public class BookingRepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();
        public async ValueTask DisposeAsync()
        {
            await _postgres.DisposeAsync();
        }

        public async ValueTask InitializeAsync()
        {
            await _postgres.StartAsync();
        }
        private AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(_postgres.GetConnectionString()).ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .Options;

            var context = new AppDbContext(options);

            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            context.Database.Migrate();
        }

        [Fact]
        public async Task CreateBooking_SavesBookingToDatabase()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var eventRrepository = new EventRepository(context);
            await eventRrepository.AddAsync(@event, token);

            var bookingRepository = new BookingRepository(context);
            var eventId = context.Events.First().Id;

            var userRepository= new UserRepository(context);
            var user = CreateUserForTest();
            await userRepository.AddAsync(user, token);

            var newBooking = Booking.CreatePending(eventId, user.Id);
            await bookingRepository.AddAsync(newBooking, token);

            await using var verifyContext = CreateContext();
            var saved = await verifyContext.Bookings.FirstOrDefaultAsync(token);

            Assert.NotNull(saved);
            Assert.Equal(eventId, saved.EventId);
        }

        [Fact]
        public async Task CreateMultipleBookings_SavesAllBookingsToDatabase()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();
            var expectedBookingsCount = 5;

            var eventRrepository = new EventRepository(context);
            await eventRrepository.AddAsync(@event, token);

            var bookingRepository = new BookingRepository(context);
            var eventId = context.Events.First().Id;

            var userRepository = new UserRepository(context);
            var user = CreateUserForTest();
            await userRepository.AddAsync(user, token);

            for (int i = 1; i <= expectedBookingsCount; i++)
            {
                var newBooking = Booking.CreatePending(eventId, user.Id);
                await bookingRepository.AddAsync(newBooking, token);
            }

            await using var verifyContext = CreateContext();
            var savedBookingRepository = new BookingRepository(verifyContext);
            var savedListBookings = await savedBookingRepository.GetAllAsync(token);

            Assert.Equal(expectedBookingsCount, savedListBookings.Count);
        }

        [Fact]
        public async Task GetById_ReturnsCorrectBooking()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var eventRrepository = new EventRepository(context);
            await eventRrepository.AddAsync(@event, token);

            var bookingRepository = new BookingRepository(context);
            var eventId = context.Events.First().Id;

            var userRepository = new UserRepository(context);
            var user = CreateUserForTest();
            await userRepository.AddAsync(user, token);

            var newBooking = Booking.CreatePending(eventId, user.Id);
            await bookingRepository.AddAsync(newBooking, token);

            await using var verifyContext = CreateContext();
            var savedBookingRepository = new BookingRepository(verifyContext);
            var savedBooking = await savedBookingRepository.GetByIdAsync(newBooking.Id, token);

            Assert.NotNull(savedBooking);
            Assert.Equal(newBooking.Id, savedBooking.Id);
        }

        [Fact]

        public async Task GetPendingAsync_ReturnBokingWithPendingStatus()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var eventRrepository = new EventRepository(context);
            await eventRrepository.AddAsync(@event, token);

            var bookingRepository = new BookingRepository(context);
            var eventId = context.Events.First().Id;

            var userRepository = new UserRepository(context);
            var user = CreateUserForTest();
            await userRepository.AddAsync(user, token);

            var newBooking1 = Booking.CreatePending(eventId, user.Id);
            await bookingRepository.AddAsync(newBooking1, token);
            var newBooking2 = Booking.CreatePending(eventId, user.Id);
            await bookingRepository.AddAsync(newBooking2, token);
            newBooking2.Status = BookingStatus.Rejected;
            await bookingRepository.SaveChangesAsync(token);

            await using var verifyContext = CreateContext();
            var savedBookingRepository = new BookingRepository(verifyContext);
            var bookingListWithPendingStatus = await savedBookingRepository.GetPendingAsync(token);

            Assert.Equal(1, bookingListWithPendingStatus.Count);
        }

        [Fact]
        public async Task AnyAsync_ReturnNotNullBooking()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var eventRrepository = new EventRepository(context);
            await eventRrepository.AddAsync(@event, token);

            var bookingRepository = new BookingRepository(context);
            var eventId = context.Events.First().Id;

            var userRepository = new UserRepository(context);
            var user = CreateUserForTest();
            await userRepository.AddAsync(user, token);

            var newBooking = Booking.CreatePending(eventId, user.Id);
            await bookingRepository.AddAsync(newBooking, token);

            await using var verifyContext = CreateContext();
            var savedBookingRepository = new BookingRepository(verifyContext);
            var savedBooking = await savedBookingRepository.AnyAsync(token);

            Assert.NotNull(savedBooking);
        }
        [Fact]
        public async Task SaveChangesAsync_CorrectUpdateBookingStatus()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var eventRrepository = new EventRepository(context);
            await eventRrepository.AddAsync(@event, token);

            var bookingRepository = new BookingRepository(context);
            var eventId = context.Events.First().Id;

            var userRepository = new UserRepository(context);
            var user = CreateUserForTest();
            await userRepository.AddAsync(user, token);

            var newBooking = Booking.CreatePending(eventId, user.Id);
            await bookingRepository.AddAsync(newBooking, token);

            await using var actContext = CreateContext();
            var savedBookingRepository = new BookingRepository(actContext);
            var savedBooking = await savedBookingRepository.GetByIdAsync(newBooking.Id, token);
            savedBooking?.Status = BookingStatus.Rejected;
            await savedBookingRepository.SaveChangesAsync(token);

            await using var verifyContext = CreateContext();
            var verifyBookingRepository = new BookingRepository(verifyContext);
            var verifyBooking = await verifyBookingRepository.GetByIdAsync(newBooking.Id, token);

            Assert.Equal(BookingStatus.Rejected, verifyBooking?.Status);
        }

        public Event CreateEventForTest()
        {
            return Event.Create
            (
                1,
                "Test event title",
                "Description test 777",
                DateTime.Now.ToUniversalTime(),
                DateTime.Now.ToUniversalTime().AddDays(1),
                100
            );
        }

        public User CreateUserForTest()
        {
            string hashPassword;
            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes("12345678");
                var hashBytes = sha256.ComputeHash(passwordBytes);

                hashPassword =  Convert.ToHexString(hashBytes);
            }
            return User.CreateUser(
                    "UserForTests",
                    hashPassword,
                    UserRoles.Admin
                );
        }
    }
}
