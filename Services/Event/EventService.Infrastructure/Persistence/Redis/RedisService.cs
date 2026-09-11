using EventService.Application.Abstractions.Services;
using EventService.Domain.Entities;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace EventService.Infrastructure.Persistence.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<RedisService> _logger;
        public RedisService(IConnectionMultiplexer connection, ILogger<RedisService> logger)
        {
            _redisDb = connection.GetDatabase();
            _logger = logger;
        }
        public async Task DeleteCacheEventFromRedis(int id)
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                var isDeleted = await _redisDb.KeyDeleteAsync($"event:{id}");
                if (isDeleted)
                    _logger.LogInformation($"Event with id - {id} successfully deleted from Redis."); 
                else
                    _logger.LogInformation($"Event with id - {id} did not delete from redis.");
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
            }
        }
        public async Task<Event?> GetCacheEventBiId(int id)
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                RedisValue value = await _redisDb.StringGetAsync($"event:{id}");

                if (value.HasValue)
                {
                    var @event = JsonSerializer.Deserialize<Event>(value.ToString());
                    return @event;
                }
                else return null;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
                return null;
            }
        }

        public async Task WriteCacheEventInRedis(Event @event)
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                string json = JsonSerializer.Serialize(@event);
                var isAdded = await _redisDb.StringSetAsync($"event:{@event.Id}", json);

                if(isAdded)
                    _logger.LogInformation($"Event with id{@event.Id} successfully added to Redis");
                else 
                    _logger.LogInformation($"Event with id{@event.Id} did not added to redis.");

            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
            }   
        }
    }
}
