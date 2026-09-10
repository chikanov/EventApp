namespace EventApp.Shared.Kafka.Contracts
{
    public class BookingRejected : IMessageContract
    {
        public Guid BookigId { get; init; }
        public int EventId { get; init; }
        public Guid UserId { get; init; }
        public int SeatsCount { get; init; }
        public DateTime ProcessingDateTime { get; init; }
        public RejectedReason Reason { get; init; }
    }
}
