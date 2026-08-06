using EventApp.Models.Enum;
using EventApp.CustomExceptions;

namespace EventService.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public int EventId {  get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Event Event {  get; set; }

        public void Confirm()
        {
            Status = BookingStatus.Confirmed;
            ProcessedAt = DateTime.Now;
        }
        public void Reject()
        {
            Status = BookingStatus.Rejected;
            ProcessedAt = DateTime.Now;
        }
        private Booking() { }

        public Booking(Guid id, int eventId, BookingStatus status, DateTime createdAt)
        {
            Id = id;
            EventId = eventId;
            Status = status;
            CreatedAt = createdAt;
        }
        public static Booking CreatePending(int eventId)
        {
            if (eventId == null)
                throw new ValidationException(nameof(EventId), "EventId cannot be empty");

            return new Booking(Guid.NewGuid(), eventId, BookingStatus.Pending, DateTime.UtcNow);
        }
    }
}
