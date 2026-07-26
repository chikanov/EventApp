using EventApp.DataAccess;
using EventApp.Models;
using EventApp.Models.Enum;
using EventApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            var newBooking = Booking.CreatePending(eventId);
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

            for (int i = 1; i <= expectedBookingsCount; i++)
            {
                var newBooking = Booking.CreatePending(eventId);
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
            var newBooking = Booking.CreatePending(eventId);
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
            var newBooking1 = Booking.CreatePending(eventId);
            await bookingRepository.AddAsync(newBooking1, token);
            var newBooking2 = Booking.CreatePending(eventId);
            await bookingRepository.AddAsync(newBooking2, token);

            await using var verifyContext = CreateContext();
            var savedBookingRepository = new BookingRepository(verifyContext);
            var bookingListWithPendingStatus = await savedBookingRepository.GetPendingAsync(token);

            Assert.Equal(2, bookingListWithPendingStatus.Count);
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
            var newBooking = Booking.CreatePending(eventId);
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
            var newBooking = Booking.CreatePending(eventId);
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
    }
}
