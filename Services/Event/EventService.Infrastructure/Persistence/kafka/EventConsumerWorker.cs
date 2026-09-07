using Confluent.Kafka;
using EventApp.Shared.Kafka;
using EventApp.Shared.Kafka.Contracts;
using EventService.Application.Abstractions.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EventService.Infrastructure.Persistence.kafka
{
    public class EventConsumerWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EventConsumerWorker> _logger;
        public EventConsumerWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration, ILogger<EventConsumerWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = logger;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() => Consume(stoppingToken), stoppingToken);
        }

        private async Task Consume(CancellationToken stoppingToken)
        {
            var bootstrapServers = _configuration.GetValue<string>("Kafka:BootstrapServers")
                ?? throw new InvalidOperationException("Kafka 'BootstrapServers' not found.");
            var groupId = _configuration.GetValue<string>("Kafka:ConsumerGroup")
                ?? throw new InvalidOperationException("Kafka 'ConsumerGroup' not found.");
            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                EnableAutoOffsetStore = false
            };
            using var consumer = new ConsumerBuilder<string, string>(config).Build();

            consumer.Subscribe(Constants.BookingConfirmed);

            _logger.LogInformation("Consumer started. Waiting for messages from a topic 'booking-cofirded'...");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    BookingConfirmed deserializedOrder = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);

                    _logger.LogInformation($"Received a message from the topic 'booking-cofirded': bookingId - {deserializedOrder.BookigId}; " +
                        $"eventId - {deserializedOrder.EventId}; userId - {deserializedOrder.UserId}; SeatsCount - {deserializedOrder.SeatsCount}; " +
                        $"processingDateTime - {deserializedOrder.ProcessingDateTime}");
                    
                    using var scope = _scopeFactory.CreateScope();
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                    var @event = await eventRepository.GetByIdAsync(deserializedOrder.EventId, stoppingToken);

                    if (@event == null)
                    {
                        _logger.LogError($"Event with Id = {deserializedOrder.EventId} does not exist.");
                        return;
                    }
                    if (@event.StartAt < deserializedOrder.ProcessingDateTime)
                    {
                        _logger.LogError("You cannot book an event that has already taken place.");
                        return;
                    }
                    if (!@event.TryReserveSeats())
                    {
                        _logger.LogError($"The available seats for the event are over.");
                        return;
                    }
                    if (@event != null && @event.StartAt > deserializedOrder.ProcessingDateTime && @event.TryReserveSeats())
                    {
                        @event.ReleaseSeats();
                        await eventRepository.SaveChangesAsync(stoppingToken);
                    }

                    consumer.StoreOffset(consumeResult);
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogInformation("Consumer is stopped normally.");
            }
            finally { consumer.Close(); }
        }
    }
}
