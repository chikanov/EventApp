using Confluent.Kafka;
using Newtonsoft.Json;
using UserService.Application.Abstractions.Persistence.KafkaContracts;
using UserService.Application.Abstractions.Services;

namespace UserService.Infrastructure.Persistence.Kafka
{
    public class KafkaProducerService : IKafkaProducerService, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private bool _disposed = false;

        public KafkaProducerService(string bootstrapServers)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            Console.WriteLine($"User kafka Producer created");
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _disposed = true;
                Console.WriteLine($"User kafka Producer disposed.");
            }
        }
        public async Task SendMessageToKafka(string topicName, IUserMessageContract message, CancellationToken ct = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(KafkaProducerService));

            if (message == null || topicName == null)
                throw new ArgumentNullException(nameof(message));

            try
            {
                var result = await _producer.ProduceAsync(topicName, new Message<string, string>
                {
                    Key = message.EventId.ToString(),
                    Value = JsonConvert.SerializeObject(message)
                }, ct);
                Console.WriteLine($"Message delivered to bookings [{result.TopicPartitionOffset}]");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to Kafka: {ex.Message}");
                throw;
            }
        }
    }
}
