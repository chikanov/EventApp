using EventApp.Shared.Kafka.Contracts;
namespace EventService.Application.Abstractions.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageToKafka(string topicName, IMessageContract message, CancellationToken ct = default);
    }
}
