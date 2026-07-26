using EventApp.DataAccess;
using EventApp.Models;
using EventApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Testcontainers.PostgreSql;

namespace EventApp.EventServiceIntegrationTests
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

            await using var verifyContext = CreateContext();
            var savedRepository = new EventRepository(verifyContext);
            var savedListEvents = await savedRepository.GetAllAsync(token);

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

            await using var verifyContext = CreateContext();
            var savedRepository = new EventRepository(verifyContext);
            var result = await savedRepository.GetByIdAsync(1, token);

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
            var actRepository = new EventRepository(actContext);
            saved?.Description = "Description test 777 - updated";
            await actRepository.UpdateAsync(saved!, token);

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
            var actRepository = new EventRepository(actContext);
            var curEvent = await actRepository.GetByIdAsync(1, token);
            await actRepository.DeleteAsync(curEvent!, token);

            await using var verifyContext = CreateContext();
            var savedRepository = new EventRepository(verifyContext);

            var deleted = await savedRepository.GetByIdAsync(1, token);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task AnyAsync_ReturnNotNullEvent()
        {
            await ResetDatabaseAsync();

            var token = new CancellationToken();
            await using var arrangeContext = CreateContext();
            var @event = CreateEventForTest();
            var repository = new EventRepository(arrangeContext);
            await repository.AddAsync(@event, token);

            await using var actContext = CreateContext();
            var actRepository = new EventRepository(actContext);
            var actEvent = await actRepository.AnyAsync(token);

            Assert.NotNull(actEvent);
        }

        [Fact]
        public async Task MaxAsync_ReturnCorrectMaxId()
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

            await using var verifyContext = CreateContext();
            var savedRepository = new EventRepository(verifyContext);
            var maxEventId = await savedRepository.MaxAsync(token);

            Assert.Equal(expectedEventsCount, maxEventId);
        }

        [Fact]

        public async Task SaveChangesAsync_ReturnCorrectUpdatedEvent()
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
            var actRepository = new EventRepository(actContext);
            saved?.Description = "Description test 777 - updated";
            await actRepository.SaveChangesAsync(token);

            await using var verifyContext = CreateContext();
            var updated = await verifyContext.Events
                .FirstOrDefaultAsync(b => b.Description == "Description test 777 - updated", cancellationToken: TestContext.Current.CancellationToken);
            Assert.Equal("Description test 777 - updated", updated?.Description);
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
                    DateTime.Now.ToUniversalTime().AddDays(i),
                    DateTime.Now.ToUniversalTime().AddDays(1 + i),
                    100
                ));
            }
            return eventsForTest;
        }
    }
}
