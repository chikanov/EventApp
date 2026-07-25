using EventApp.DataAccess;
using EventApp.Models;
using EventApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace EventApp.EventServiceIntagrationTests
{
    public class EventRepositoryIntegrationTests : IAsyncLifetime
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
                .UseNpgsql(_postgres.GetConnectionString())
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private async Task ResetDatabaseAsync()
        {
            await using var context = CreateContext();
            await context.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE events, bookings RESTART IDENTITY CASCADE");
        }

        [Fact]
        public async Task CreateEvent_SavesEventToDatabase()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var repository = new EventRepository(context);
            await repository.AddAsync(@event, token);

            await using var verifyContext = CreateContext();
            var saved = await verifyContext.Events
                .FirstOrDefaultAsync(b => b.Description == "Description test 777", cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(saved);
            Assert.Equal("Test event title", saved.Title);
        }

        [Fact]
        public async Task CreateMultipleEvents_SavesAllEventsToDatabase_ReturnCorrectCount()
        {
            await ResetDatabaseAsync();
            await using var context = CreateContext();
            var token = new CancellationToken();
            var expectedEventsCount = 5;
            var listEvents = CreateMultipleEventsForTest(expectedEventsCount);

            var repository = new EventRepository(context);
            foreach (var @event in listEvents)
            {
                await repository.AddAsync(@event, token);
            }

            var savedListEvents = await repository.GetAllAsync(token);

            Assert.Equal(expectedEventsCount, savedListEvents.Count);
        }

        [Fact]
        public async Task GetById_ReturnsCorrectEvent()
        {
            await ResetDatabaseAsync();

            await using var context = CreateContext();
            var token = new CancellationToken();
            var @event = CreateEventForTest();

            var repository = new EventRepository(context);
            await repository.AddAsync(@event, token);

            var result = await repository.GetByIdAsync(1, token);

            Assert.NotNull(result);
            Assert.Equal("Description test 777", result.Description);
        }

        [Fact]
        public async Task UpdateEvent_ChangesFieldInDatabase()
        {
            await ResetDatabaseAsync();
            var token = new CancellationToken();

            await using var arrangeContext = CreateContext();
            var @event = CreateEventForTest();

            var repository = new EventRepository(arrangeContext);
            await repository.AddAsync(@event, token);

            await using var actContext = CreateContext();
            var saved = await actContext.Events
                .FirstOrDefaultAsync(b => b.Description == "Description test 777", cancellationToken: TestContext.Current.CancellationToken);
            saved?.Description = "Description test 777 - updated";
            actContext.Events.Update(saved!);
            await actContext.SaveChangesAsync(token);

            await using var verifyContext = CreateContext();
            var updated = await verifyContext.Events
                .FirstOrDefaultAsync(b => b.Description == "Description test 777 - updated", cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("Description test 777 - updated", updated?.Description);
        }

        [Fact]
        public async Task DeleteEvent_RemovesFromDatabase()
        {
            await ResetDatabaseAsync();

            var token = new CancellationToken();
            await using var arrangeContext = CreateContext();
            var @event = CreateEventForTest();
            var repository = new EventRepository(arrangeContext);
            await repository.AddAsync(@event, token);

            await using var actContext = CreateContext();
            var curEvent = await repository.GetByIdAsync(1, token);
            await repository.DeleteAsync(curEvent!, token);

            await using var verifyContext = CreateContext();
            var deleted = await repository.GetByIdAsync(1, token);
            Assert.Null(deleted);
        }

        public Event CreateEventForTest()
        {
            return Event.Create
            (
                1,
                "Test event title",
                "Description test 777",
                new DateTime().ToUniversalTime(),
                new DateTime().ToUniversalTime().AddDays(1),
                100
            );
        }

        public List<Event> CreateMultipleEventsForTest(int count)
        {
            List<Event> eventsForTest = new List<Event>();
            for (int i = 1; i <= count; i++)
            {
                eventsForTest.Add(Event.Create
                (
                    i,
                    "Test event title " + i,
                    "Description test " + i,
                    new DateTime().ToUniversalTime().AddDays(i),
                    new DateTime().ToUniversalTime().AddDays(1 + i),
                    100
                ));
            }
            return eventsForTest;
        }
    }
}
