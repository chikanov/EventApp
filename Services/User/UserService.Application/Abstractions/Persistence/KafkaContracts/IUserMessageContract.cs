namespace UserService.Application.Abstractions.Persistence.KafkaContracts
{
    public interface IUserMessageContract
    {
        public Guid BookigId { get; init; }
        public int EventId { get; init; }
        public Guid UserId { get; init; }
        public int SeatsCount { get; init; }
        public DateTime ProcessingDateTime { get; init; }
    }
}
