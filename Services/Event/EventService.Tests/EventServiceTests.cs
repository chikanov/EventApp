using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Application.Abstractions.Services;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using Moq;

namespace EventService.Tests
{
    public class EventServiceTests 
    {
        private Mock<IEventRepository> _mockRepository;
        private Mock<IRedisService> _mockCache;
        private Application.Services.EventService _service;
        public EventServiceTests()
        {
            _mockRepository = new Mock<IEventRepository>();
            _mockCache = new Mock<IRedisService>();
            _service = new Application.Services.EventService(_mockRepository.Object, _mockCache.Object);
        }
        [Fact]
        public async Task GetEvent_WhenEventInCache_RepositoryNotCalled()
        {
            // Arrange
            var eventId = 1;
            var token = new CancellationToken();

            var curEvent = Event.Create("Test event title", "Description test 777", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            curEvent.Id = eventId;

            _mockCache.Setup(x => x.GetCacheEventByIdAsync(curEvent.Id))
                   .ReturnsAsync(curEvent);
            _mockRepository.Setup(x => x.GetByIdAsync(eventId))
                         .ReturnsAsync((Event)null);

            // Act
            var result = await _service.GetByIdAsync(curEvent.Id);

            // Assert
            Assert.Equal(curEvent, result);
            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>()), Times.Never);
            _mockCache.Verify(x => x.GetCacheEventByIdAsync(curEvent.Id), Times.Once);
        }

        [Fact]
        public async Task GetEventAsync_WhenNotInCache_GetsFromRepositoryAndCaches()
        {
            // Arrange
            int eventId = 2;
            var expectedEvent = Event.Create("Test event title", "Description test 777", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 10);
            expectedEvent.Id = eventId;

            _mockCache.Setup(x => x.GetCacheEventByIdAsync(expectedEvent.Id))
                      .ReturnsAsync((Event)null);
            _mockRepository.Setup(x => x.GetByIdAsync(eventId))
                         .ReturnsAsync(expectedEvent);

            // Act
            var result = await _service.GetByIdAsync(eventId);

            // Assert
            Assert.Equal(expectedEvent, result);
            _mockRepository.Verify(x => x.GetByIdAsync(eventId), Times.Once);
            _mockCache.Verify(x => x.WriteCacheEventInRedisAsync(expectedEvent), Times.Once);
        }

        [Fact]
        public async Task UodateEventAsync_InvalidatesCache()
        {
            // Arrange
            int eventId = 3;
            var EventDto = new EventDto() { Title = "Test event title - updated",
                                                 Description = "Description test 777-updated",
                                                 StartAt = DateTime.UtcNow,
                                                 EndAt = DateTime.UtcNow.AddDays(1),
                                                 TotalSeats = 10
            };
            var expectedEvent = Event.Create("Test event title", "Description test 777", DateTime.UtcNow,
                DateTime.UtcNow.AddDays(1), 10);
            expectedEvent.Id = eventId;
            _mockRepository.Setup(x => x.GetByIdAsync(eventId))
                         .ReturnsAsync(expectedEvent);

            // Act
            await _service.UpdateEventAsync(eventId, EventDto);

            // Assert
            _mockRepository.Verify(x => x.UpdateAsync(EventDto, expectedEvent), Times.Once);
            _mockCache.Verify(x => x.DeleteCacheEventFromRedisAsync(eventId), Times.Once);
            _mockCache.Verify(x => x.WriteCacheEventInRedisAsync(expectedEvent), Times.Once);
        }
    }
}
