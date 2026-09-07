using EventService.Application.Abstractions.Persistence.KafkaContracts;

namespace EventService.Application.Abstractions.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageToKafka(string topicName, IEventMessageContract message, CancellationToken ct = default);
    }
}
