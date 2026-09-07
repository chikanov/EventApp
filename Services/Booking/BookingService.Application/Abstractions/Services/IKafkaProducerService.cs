using BookingService.Application.Abstractions.Persistence.KafkaContracts;

namespace BookingService.Application.Abstractions.Services
{
    public interface IKafkaProducerService
    {
        Task SendMessageToKafka(string topicName, IBookingMessageContract message, CancellationToken ct = default);
    }
}
