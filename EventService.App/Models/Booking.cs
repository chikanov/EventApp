using System.ComponentModel.DataAnnotations;

namespace EventApp.Models
{
    public class Booking
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public int EventId {  get; set; }
        [Required]
        public string Status { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        public enum BookingStatus
        {
            Pending, Confirmed, Rejected
        }

        public void Confirm()
        {
            Status = BookingStatus.Confirmed.ToString();
            ProcessedAt = DateTime.Now;
        }
        public void Reject()
        {
            Status = BookingStatus.Rejected.ToString();
            ProcessedAt = DateTime.Now;
        }
    }
}
