namespace EventApp.Shared.Kafka.Contracts
{
    public class BookingCancelled
    {
        public Guid BookigId { get; init; }
        public int EventId { get; init; }
        public Guid UserId { get; init; }
        public int SeatsCount { get; init; }
        public DateTime CanceledDate { get; init; }
    }
}
