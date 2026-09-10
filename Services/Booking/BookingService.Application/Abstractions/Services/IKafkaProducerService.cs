using EventApp.Shared.Kafka.Contracts;

namespace BookingService.Application.Abstractions.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageToKafka(string topicName, IMessageContract message, CancellationToken ct = default);
    }
}
