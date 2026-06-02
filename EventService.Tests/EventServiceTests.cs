using EventApp.CustomExceptions;
using EventApp.Models;
using EventApp.Models.DTO;
using EventApp.Services;
using System.ComponentModel.DataAnnotations;
namespace EventApp
{
    public class EventServiceTests : IClassFixture<EventServiceFixture>
    {
        private readonly EventService _eventService;
        public EventServiceTests(EventServiceFixture fixture)
        {
            _eventService = fixture.eventService;
        }

        [Fact]
        public void CreateEventTest_ReturnEventNotNull()
        {
            var eventDto = new EventDto()
            {
                Description = "Description test 777",
                EndAt = new DateTime().AddDays(1),
                StartAt = new DateTime(),
                Title = "Test event title" };

            var result = _eventService.Add(eventDto);

            Assert.NotNull(result);
        }

        [Fact]
        public void GetAllEventsTest_ReturnListEvents()
        {
            var expectedEvents = new List<Event>()
            {
                 new Event(){ Id = 1, Title = "Tittle1", Description = "Description1", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)},
            new Event(){ Id = 2, Title = "Tittle2", Description = "Description2", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)},
            new Event(){ Id = 3, Title = "Tittle3", Description = "Description3", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2)},
            new Event(){ Id = 4, Title = "Tittle4", Description = "Description4", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2)},
            new Event(){ Id = 5, Title = "Tittle5", Description = "Description5", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3)},
            new Event(){ Id = 6, Title = "Tittle6", Description = "Description6", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3)},
            new Event(){ Id = 7, Title = "Tittle7", Description = "Description7", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4)},
            new Event(){ Id = 8, Title = "Tittle8", Description = "Description8", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4)},
            new Event(){ Id = 9, Title = "Tittle9", Description = "Description9", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5)},
            new Event(){ Id = 10, Title = "Tittle10", Description = "Description10", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5)},
            new Event(){ Id = 11, Title = "Tittle11", Description = "Description11", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7)},
            new Event(){ Id = 12, Title = "Tittle12", Description = "Description12", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7)},
            new Event(){ Id = 13, Title = "Tittle13", Description = "Description13", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9)},
            new Event(){ Id = 14, Title = "Tittle14", Description = "Description14", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9)},
            new Event(){ Id = 15, Title = "Tittle15", Description = "Description15", StartAt = DateTime.Now.AddDays(9), EndAt = DateTime.Now.AddDays(10)}
            };

            var result = _eventService.GetAll(1, 15).ListEvents.Where(e => e.Id <= 15).ToList();

            Assert.Equal(expectedEvents.Count, result.Count);
        }

        [Fact]
        public void GetEventByIdTest_ReturnEventById()
        {
            var expectedId = 5;

            var result = _eventService.GetById(expectedId);

            Assert.Equal(expectedId, result?.Id);
        }

        [Fact]
        public void UpdateEventTest_ReturnUpdatedEvent()
        {
            var expextedEventId = 5;
            var eventDto = new EventDto() { 
                Title = "Tittle5 - updated", 
                Description = "Description5 - updated", 
                StartAt = DateTime.Now.AddDays(3), 
                EndAt = DateTime.Now.AddDays(4) };

            var result = _eventService.Update(expextedEventId, eventDto);

            Assert.Equal(eventDto.Title, result.Title);
            Assert.Equal(eventDto.Description, result.Description);
            Assert.Equal(eventDto.StartAt, result.StartAt);
            Assert.Equal(eventDto.EndAt, result.EndAt);
        }

        [Fact]
        public void DeleteEventTest()
        {
            var expextedEventId = 5;

            _eventService.Delete(expextedEventId);

            var result = _eventService.GetById(expextedEventId);

            Assert.Null(result);
        }

        [Fact]
        public void FiltringEventsTest_ReturnFiltredEventsByTitleByStartAtByEndAt()
        {
            var expectedTitle = "Title1";
            var notExpectedTitle = "Title6";
            var expectedStartAt = _eventService?.GetById(1)?.StartAt;
            var expectedEndAt = _eventService?.GetById(1)?.EndAt;

            var result = _eventService?.GetAll(1, 10, expectedTitle, expectedStartAt, expectedEndAt);

            Assert.All(result.ListEvents, events => expectedTitle.Contains(events.Title));
            Assert.DoesNotContain(notExpectedTitle, result.ListEvents.Select(events => events.Title));
        }

        [Fact]
        public void FiltringEventsTest_ReturnFiltredEventsByTitle()
        {
            var expectedTitle = "Title1";
            var notExpectedTitle = "Title6";

            var result = _eventService?.GetAll(1, 10, expectedTitle);

            Assert.All(result.ListEvents, events => expectedTitle.Contains(events.Title));
            Assert.DoesNotContain(notExpectedTitle, result.ListEvents.Select(events => events.Title));
        }

        [Fact]
        public void FiltringEventsTest_ReturnFiltredEventsByStartAtByEndAt()
        {
            var expectedStartAt = _eventService?.GetById(1)?.StartAt;
            var expectedEndAt = _eventService?.GetById(1)?.EndAt;
            var expectedEventsCount = 2;

            var result = _eventService?.GetAll(1, 10, null, expectedStartAt, expectedEndAt);

            Assert.Equal(expectedEventsCount, result.EventsCount);
        }

        [Fact]
        public void PaginationEventTest_ReturnPageNumberPgageCountEvents()
        {
            var expectedPageNumber = 2;
            var expectedPageCount = 5;
            var expectedListEventsTitles = new List<string> { "Title11", "Title12", "Title13", "Title14", "Title15" };

            var result = _eventService?.GetAll(expectedPageNumber, expectedPageCount);

            Assert.Equal(expectedPageCount, result?.ListEvents.Count());
            Assert.All(result!.ListEvents, events => expectedListEventsTitles.Contains(events.Title));
        }

        [Fact]
        public void GetEventByNotExistIdTest_ReturnNull()
        {
            var expecteNotExistId = -1;

            var result = _eventService?.GetById(expecteNotExistId);

            Assert.Null(result);
        }

        [Fact]
        public void UpdateEventWithNotExistId_ReturnNull()
        {
            var expectedNotExistId = -1;
            var expectedParamName = $"Event with Id = {expectedNotExistId} does not exist.";
            var eventDto = new EventDto()
            {
                Title = "Tittle5 - updated",
                Description = "Description5 - updated",
                StartAt = DateTime.Now.AddDays(1),
                EndAt = DateTime.Now.AddDays(2)
            };

            var exception = Assert
        .Throws<NotFoundException>(() => _eventService?.Update(expectedNotExistId, eventDto));

            Assert.Equal(expectedParamName, exception.Message);
        }

        [Fact]
        public void UpdateEventWithNotCorrectDate_ReturnNull()
        {
            var expectedId = 5;
            var expectedParamName = "The end date must be greater than the start date.";
            var eventDto = new EventDto()
            {
                Title = "Tittle5 - updated",
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
