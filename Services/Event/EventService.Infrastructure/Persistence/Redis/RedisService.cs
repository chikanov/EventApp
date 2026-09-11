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
        private const int eventTtlMinutes = 10;
        private const string topEventsKey = "events:top10";
        public RedisService(IConnectionMultiplexer connection, ILogger<RedisService> logger)
        {
            _redisDb = connection.GetDatabase();
            _logger = logger;
        }
        public async Task DeleteCacheEventFromRedisAsync(int id)
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                var isDeleted = await _redisDb.KeyDeleteAsync($"event:{id}");
                if (isDeleted)
                    _logger.LogInformation($"Event with id - {id} successfully deleted from Redis."); 
                else
                    _logger.LogInformation($"Event with id - {id} did not deleted from redis.");
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
            }
        }
        public async Task<Event?> GetCacheEventByIdAsync(int id)
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

        public async Task WriteCacheEventInRedisAsync(Event @event)
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                string json = JsonSerializer.Serialize(@event);
                var isAdded = await _redisDb.StringSetAsync($"event:{@event.Id}", json, TimeSpan.FromMinutes(eventTtlMinutes));

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

        public async Task<List<Event>> GetTopCacheEventsAsync()
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                RedisValue value = await _redisDb.StringGetAsync(topEventsKey);

                if (value.HasValue)
                {
                    var listEvents = JsonSerializer.Deserialize<List<Event>>(value.ToString());
                    return listEvents!;
                }
                else return null!;
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
                return null!;
            }
        }

        public async Task WriteCacheTopEventsInRedisAsync(List<Event> topEvents)
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                string json = JsonSerializer.Serialize(topEvents);
                var isAdded = await _redisDb.StringSetAsync(topEventsKey, json, TimeSpan.FromMinutes(eventTtlMinutes));

                if (isAdded)
                    _logger.LogInformation("Top 10 Events successfully added to Redis");
                else
                    _logger.LogInformation("Top 10 Events did not added to redis.");

            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
            }
        }

        public async Task DeleteCacheTopEventsFromRedisAsync()
        {
            try
            {
                await _redisDb.PingAsync();
                _logger.LogInformation("Redis is available.");

                var isDeleted = await _redisDb.KeyDeleteAsync(topEventsKey);
                if (isDeleted)
                    _logger.LogInformation("Top 10 Events successfully deleted from Redis.");
                else
                    _logger.LogInformation("Top 10 Events did not deleted from redis.");
            }
            catch (RedisConnectionException ex)
            {
                _logger.LogError($"Couldn't connect to Redis: {ex.Message}");
            }
        }
    }
}
