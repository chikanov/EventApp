using UserService.Application.Abstractions.Persistence.KafkaContracts;

namespace UserService.Application.Abstractions.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageToKafka(string bootstrapServers, string topicName, IUserMessageContract message, CancellationToken ct = default);
    }
}
