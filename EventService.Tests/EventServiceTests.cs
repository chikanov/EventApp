using EventApp.CustomExceptions;
using EventApp.Models;
using EventApp.Models.DTO;
using System.ComponentModel.DataAnnotations;
using Xunit.v3.Priority;

namespace EventApp.Services
{
    [TestCaseOrderer(typeof(PriorityOrderer))]
    public class EventServiceTests : IClassFixture<EventServiceFixture>
    {
        private readonly EventService _eventService;
        public EventServiceTests(EventServiceFixture fixture)
        {
            _eventService = fixture.eventService;
        }

        [Fact, Priority(0)]
        public void CreateEventTest_ReturnEventNotNull()
        {
            var eventDto = new CreateEventDto()
            {
                Description = "Description test 777",
                EndAt = new DateTime().AddDays(1),
                StartAt = new DateTime(),
                Title = "Test event title",
                TotalSeats = 100 };

            var result = _eventService.Add(eventDto);

            Assert.NotNull(result);
        }

        [Fact, Priority(1)]
        public void GetAllEventsTest_ReturnListEvents()
        {
            var expectedEvents = new List<Event>()
            {
                 new Event(100){ Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1), TotalSeats = 100},
            new Event(100){ Id = 2, Title = "Title2", Description = "Description2", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1), TotalSeats = 100},
            new Event(100){ Id = 3, Title = "Title3", Description = "Description3", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2), TotalSeats = 100},
            new Event(100){ Id = 4, Title = "Title4", Description = "Description4", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2), TotalSeats = 100},
            new Event(100){ Id = 5, Title = "Title5", Description = "Description5", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3), TotalSeats = 100},
            new Event(100){ Id = 6, Title = "Title6", Description = "Description6", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3), TotalSeats = 100},
            new Event(100){ Id = 7, Title = "Title7", Description = "Description7", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4), TotalSeats = 100},
            new Event(100){ Id = 8, Title = "Title8", Description = "Description8", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4), TotalSeats = 100},
            new Event(100){ Id = 9, Title = "Title9", Description = "Description9", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5), TotalSeats = 100},
            new Event(100){ Id = 10, Title = "Title10", Description = "Description10", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5), TotalSeats = 100},
            new Event(100){ Id = 11, Title = "Title11", Description = "Description11", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7), TotalSeats = 100},
            new Event(100){ Id = 12, Title = "Title12", Description = "Description12", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7), TotalSeats = 100},
            new Event(100){ Id = 13, Title = "Title13", Description = "Description13", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9), TotalSeats = 100},
            new Event(100){ Id = 14, Title = "Title14", Description = "Description14", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9), TotalSeats = 100},
            new Event(100){ Id = 15, Title = "Title15", Description = "Description15", StartAt = DateTime.Now.AddDays(9), EndAt = DateTime.Now.AddDays(10), TotalSeats = 100}
            };

            var result = _eventService.GetAll(1, 15).ListEvents.Where(e => e.Id <= 15).ToList();

            Assert.Equal(expectedEvents.Count, result.Count);
        }

        [Fact, Priority(2)]
        public void GetEventByIdTest_ReturnEventById()
        {
            var expectedId = 5;

            var result = _eventService.GetById(expectedId);

            Assert.Equal(expectedId, result?.Id);
        }

        [Fact, Priority(3)]
        public void UpdateEventTest_ReturnUpdatedEvent()
        {
            var expextedEventId = 5;
            var eventDto = new EventDto() { 
                Title = "Title5 - updated", 
                Description = "Description5 - updated", 
                StartAt = DateTime.Now.AddDays(3), 
                EndAt = DateTime.Now.AddDays(4),
                TotalSeats = 100 };

            var result = _eventService.Update(expextedEventId, eventDto);

            Assert.Equal(eventDto.Title, result.Title);
            Assert.Equal(eventDto.Description, result.Description);
            Assert.Equal(eventDto.StartAt, result.StartAt);
            Assert.Equal(eventDto.EndAt, result.EndAt);
        }

        [Fact, Priority(4)]
        public void DeleteEventTest()
        {
            var expextedEventId = 5;

            _eventService.Delete(expextedEventId);

            var result = _eventService.GetById(expextedEventId);

            Assert.Null(result);
        }

        [Fact, Priority(5)]
        public void FiltringEventsTest_ReturnFiltredEventsByTitleByStartAtByEndAt()
        {
            var expectedTitle = "Title2";
            var notExpectedTitle = "Title6";
            var expectedStartAt = _eventService?.GetById(2)?.StartAt;
            var expectedEndAt = _eventService?.GetById(2)?.EndAt;

            var result = _eventService?.GetAll(1, 10, expectedTitle, expectedStartAt, expectedEndAt);

            Assert.All(result.ListEvents, events => expectedTitle.Contains(events.Title));
            Assert.DoesNotContain(notExpectedTitle, result.ListEvents.Select(events => events.Title));
        }

        [Fact, Priority(6)]
        public void FiltringEventsTest_ReturnFiltredEventsByTitle()
        {
            var expectedTitle = "Title2";
            var notExpectedTitle = "Title6";

            var result = _eventService?.GetAll(1, 10, expectedTitle);

            Assert.All(result.ListEvents, events => expectedTitle.Contains(events.Title));
            Assert.DoesNotContain(notExpectedTitle, result.ListEvents.Select(events => events.Title));
        }

        [Fact, Priority(7)]
        public void FiltringEventsTest_ReturnFiltredEventsByStartAtByEndAt()
        {
            var expectedEvent = new CreateEventDto() { Title = "Expected Event title", Description = "Expected Event description",
                StartAt = DateTime.Now.AddDays(100), EndAt = DateTime.Now.AddDays(101), TotalSeats = 100 };

            _eventService.Add(expectedEvent);
            var expectedStartAt = _eventService?.GetById(16)?.StartAt;
            var expectedEndAt = _eventService?.GetById(16)?.EndAt;
            var expectedEventsCount = 1;

            var result = _eventService?.GetAll(1, 10, null, expectedStartAt, expectedEndAt);

            Assert.Equal(expectedEventsCount, result.EventsCount);
        }

        [Fact, Priority(8)]
        public void PaginationEventTest_ReturnPageNumberPgageCountEvents()
        {
            var expectedPageNumber = 2;
            var expectedPageCount = 5;
            var expectedListEventsTitles = new List<string> { "Title11", "Title12", "Title13", "Title14", "Title15" };

            var result = _eventService?.GetAll(expectedPageNumber, expectedPageCount);

            Assert.Equal(expectedPageCount, result?.ListEvents.Count());
            Assert.All(result!.ListEvents, events => expectedListEventsTitles.Contains(events.Title));
        }

        [Fact, Priority(9)]
        public void GetEventByNotExistIdTest_ReturnNull()
        {
            var expecteNotExistId = -1;

            var result = _eventService?.GetById(expecteNotExistId);

            Assert.Null(result);
        }

        [Fact, Priority(10)]
        public void UpdateEventWithNotExistId_ReturnNull()
        {
            var expectedNotExistId = -1;
            var expectedParamName = $"Event with Id = {expectedNotExistId} does not exist.";
            var eventDto = new EventDto()
            {
                Title = "Title5 - updated",
                Description = "Description5 - updated",
                StartAt = DateTime.Now.AddDays(1),
                EndAt = DateTime.Now.AddDays(2)
            };

            var exception = Assert
        .Throws<NotFoundException>(() => _eventService?.Update(expectedNotExistId, eventDto));

            Assert.Equal(expectedParamName, exception.Message);
        }

        [Fact, Priority(11)]
        public void UpdateEventWithNotCorrectDate_ReturnNull()
        {
            var expectedId = 9;
            var expectedParamName = "The end date must be greater than the start date.";
            var eventDto = new EventDto()
            {
                Title = "Title5 - updated",
                Description = "Description5 - updated",
                StartAt = DateTime.Now.AddDays(3),
                EndAt = DateTime.Now.AddDays(2)
            };

            var exception = Assert
        .Throws<ValidationException>(() => _eventService?.Update(expectedId, eventDto));

            Assert.Equal(expectedParamName, exception.Message);
        }
    }
}
