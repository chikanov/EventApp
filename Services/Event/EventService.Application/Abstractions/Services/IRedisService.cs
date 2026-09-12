using EventService.Domain.Entities;

namespace EventService.Application.Abstractions.Services
{
    public interface IRedisService
    {
        Task<Event?> GetCacheEventByIdAsync(int id);
        Task<List<Event>> GetTopCacheEventsAsync();
        Task WriteCacheEventInRedisAsync(Event @event);
        Task WriteCacheTopEventsInRedisAsync(List<Event> topEvents);
        Task DeleteCacheEventFromRedisAsync(int id);
        Task DeleteCacheTopEventsFromRedisAsync();
    }
}
