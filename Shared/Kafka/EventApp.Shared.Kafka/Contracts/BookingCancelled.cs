using BookingService.Application.Abstractions.Persistence.KafkaContracts;
using EventService.Application.Abstractions.Persistence.KafkaContracts;
using UserService.Application.Abstractions.Persistence.KafkaContracts;

namespace EventApp.Shared.Kafka.Contracts
{
    public class BookingCancelled : IUserMessageContract, IEventMessageContract, IBookingMessageContract
    {
        public Guid BookigId { get; init; }
        public int EventId { get; init; }
        public Guid UserId { get; init; }
        public int SeatsCount { get; init; }
        public DateTime ProcessingDateTime { get; init; }
    }
}
