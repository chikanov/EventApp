using BookingService.Domain.CustomExceptions;
using BookingService.Domain.Entities.Enum;

namespace BookingService.Domain.Entities
{
    public class Booking
    {
        public Guid Id { get; set; }
        public int EventId { get; set; }
        public Guid UserId { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        private readonly object _StatusLock = new object();

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

        public void Cancel()
        {
            lock (_StatusLock)
            {
                if (Status != BookingStatus.Cancelled)
                {
                    Status = BookingStatus.Cancelled;
                    ProcessedAt = DateTime.Now;
                }
                else
                {  
                    throw new ValidationBookingException(nameof(Status), "The status is already in a canceled state.");
                }
            }
        }
        private Booking() { }

        public Booking(Guid id, int eventId, Guid userId, BookingStatus status, DateTime createdAt)
        {
            Id = id;
            EventId = eventId;
            UserId = userId;
            Status = status;
            CreatedAt = createdAt;
        }
        public static Booking CreatePending(int eventId, Guid userId)
        {
            return new Booking(Guid.NewGuid(), eventId, userId, BookingStatus.Pending, DateTime.UtcNow);
        }
    }
}
