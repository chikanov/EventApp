using EventApp.CustomExceptions;
using EventApp.DataAccess;
using EventApp.Interfaces;
using EventApp.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.v3.Priority;

namespace EventApp.Services
{
    [TestCaseOrderer(typeof(PriorityOrderer))]
    public class EventServiceTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IServiceScope _scope;
        private readonly IEventService _eventService;
        public EventServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventService, EventService>();

            _serviceProvider = services.BuildServiceProvider();
            _scope = _serviceProvider.CreateScope();
            _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        }

        public void Dispose()
        {
            _scope.Dispose();
            _serviceProvider.Dispose();
        }

        [Fact, Priority(0)]
        public async Task CreateEventTest_ReturnEventNotNull()
        {
            var token = new CancellationToken();
            var eventDto = new CreateEventDto()
            {
                Description = "Description test 777",
                EndAt = new DateTime().AddDays(1),
                StartAt = new DateTime(),
                Title = "Test event title",
                TotalSeats = 100 };

            var result = await _eventService.CreateEventAsync(eventDto, token);

            Assert.NotNull(result);
        }

        [Fact, Priority(1)]
        public async Task GetAllEventsTest_ReturnListEvents()
        {
            var token = new CancellationToken();
            await CreateEventsForTestsAsync();
            var expectedEventsCounts = 15;
            var result = await _eventService.GetAllAsync(1, 15, null, null, null, token);

            Assert.Equal(expectedEventsCounts, result.EventsCount);
        }

        [Fact, Priority(2)]
        public async Task GetEventByIdTest_ReturnEventById()
        {
            await CreateEventsForTestsAsync();
            var expectedId = 5;
            var token = new CancellationToken();

            var result = await _eventService.GetByIdAsync(expectedId, token);

            Assert.Equal(expectedId, result?.Id);
        }

        [Fact, Priority(3)]
        public async Task UpdateEventTest_ReturnUpdatedEvent()
        {
            await CreateEventsForTestsAsync();
            var expextedEventId = 5;
            var token = new CancellationToken();
            var eventDto = new EventDto() { 
                Title = "Title5 - updated", 
                Description = "Description5 - updated", 
                StartAt = DateTime.Now.AddDays(3), 
                EndAt = DateTime.Now.AddDays(4),
                TotalSeats = 100 };

            var result = await _eventService.UpdateEventAsync(expextedEventId, eventDto, token);

            Assert.Equal(eventDto.Title, result.Title);
            Assert.Equal(eventDto.Description, result.Description);
            Assert.Equal(eventDto.StartAt, result.StartAt);
            Assert.Equal(eventDto.EndAt, result.EndAt);
        }

        [Fact, Priority(4)]
        public async Task DeleteEventTest()
        {
            await CreateEventsForTestsAsync();
            var expextedEventId = 5;
            var token = new CancellationToken();

            var result = await _eventService.DeleteEventAsync(expextedEventId, token);

            Assert.True(result);
        }

        [Fact, Priority(5)]
        public async Task FiltringEventsTest_ReturnFiltredEventsByTitleByStartAtByEndAt()
        {
            await CreateEventsForTestsAsync();
            var expectedTitle = "Title2";
            var notExpectedTitle = "Title6";
            var token = new CancellationToken();
            var expectedEvent = await _eventService.GetByIdAsync(2, token);
            var expectedStartAt = expectedEvent?.StartAt;
            var expectedEndAt = expectedEvent?.EndAt;

            var result = await _eventService.GetAllAsync(1, 10, expectedTitle, expectedStartAt, expectedEndAt, token);

            Assert.All(result!.ListEvents, events => expectedTitle.Contains(events.Title));
            Assert.DoesNotContain(notExpectedTitle, result.ListEvents.Select(events => events.Title));
        }

        [Fact, Priority(6)]
        public async Task FiltringEventsTest_ReturnFiltredEventsByTitle()
        {
            await CreateEventsForTestsAsync();
            var expectedTitle = "Title2";
            var notExpectedTitle = "Title6";
            var token = new CancellationToken();

            var result = await _eventService.GetAllAsync(1, 10, expectedTitle,null, null, token);

            Assert.All(result!.ListEvents, events => expectedTitle.Contains(events.Title));
            Assert.DoesNotContain(notExpectedTitle, result.ListEvents.Select(events => events.Title));
        }

        [Fact, Priority(7)]
        public async Task FiltringEventsTest_ReturnFiltredEventsByStartAtByEndAt()
        {
            var token = new CancellationToken();
            var newEvent = new CreateEventDto() { Title = "Expected Event title", Description = "Expected Event description",
                StartAt = DateTime.Now.AddDays(100), EndAt = DateTime.Now.AddDays(101), TotalSeats = 100 };

            var expectedEvent = await _eventService.CreateEventAsync(newEvent, token);
            var expectedStartAt = expectedEvent.StartAt;
            var expectedEndAt = expectedEvent.EndAt;
            var expectedEventsCount = 1;

            var result = await _eventService.GetAllAsync(1, 10, null, expectedStartAt, expectedEndAt, token);

            Assert.Equal(expectedEventsCount, result!.EventsCount);
        }

        [Fact, Priority(8)]
        public async Task PaginationEventTest_ReturnPageNumberPgageCountEvents()
        {
            await CreateEventsForTestsAsync();
            var expectedPageNumber = 2;
            var expectedPageCount = 5;
            var expectedListEventsTitles = new List<string> { "Title11", "Title12", "Title13", "Title14", "Title15" };
            var token = new CancellationToken();

            var result = await _eventService.GetAllAsync(expectedPageNumber, expectedPageCount, null, null, null, token);

            Assert.Equal(expectedPageCount, result?.ListEvents.Count());
            Assert.All(result!.ListEvents, events => expectedListEventsTitles.Contains(events.Title));
        }

        [Fact, Priority(9)]
        public async Task GetEventByNotExistIdTest_ReturnNotFoundException()
        {
            var expecteNotExistId = -1;
            var expectedExceptionMessage = "Event not found";
            var token = new CancellationToken();

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _eventService.GetByIdAsync(expecteNotExistId, token));

            Assert.Equal(expectedExceptionMessage, exception.Message);
        }

        [Fact, Priority(10)]
        public async Task UpdateEventWithNotExistId_ReturnNull()
        {
            var expectedNotExistId = -1;
            var token = new CancellationToken();
            var expectedParamName = $"Event with Id = {expectedNotExistId} does not exist.";
            var eventDto = new EventDto()
            {
                Title = "Title5 - updated",
                Description = "Description5 - updated",
                StartAt = DateTime.Now.AddDays(1),
                EndAt = DateTime.Now.AddDays(2)
            };

            var exception = await Assert
        .ThrowsAsync<NotFoundException>(async () => await _eventService.UpdateEventAsync(expectedNotExistId, eventDto, token));

            Assert.Equal(expectedParamName, exception.Message);
        }

        [Fact, Priority(11)]
        public async Task UpdateEventWithNotCorrectDate_ReturnNull()
        {
            await CreateEventsForTestsAsync();
            var expectedId = 9;
            var token = new CancellationToken();
            var expectedParamName = "Validation failed";
            var eventDto = new EventDto()
            {
                Title = "Title5 - updated",
                Description = "Description5 - updated",
                StartAt = DateTime.Now.AddDays(3),
                EndAt = DateTime.Now.AddDays(2)
            };

            var exception = await Assert
        .ThrowsAsync<ValidationException>(async() => await _eventService.UpdateEventAsync(expectedId, eventDto, token));

            Assert.Equal(expectedParamName, exception.Message);
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
