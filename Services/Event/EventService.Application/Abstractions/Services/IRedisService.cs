using EventService.Domain.Entities;

namespace EventService.Application.Abstractions.Services
{
    public interface IRedisService
    {
        Task<Event?> GetCacheEventBiId(int id);
        Task WriteCacheEventInRedis(Event @event);
        Task DeleteCacheEventFromRedis(int id);
    }
}
