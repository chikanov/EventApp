using System.ComponentModel.DataAnnotations;

namespace EventApp.Models
{
    /// <summary>
    /// Event
    /// </summary>
    public class Event
    {
        /// Id
        public int Id { get; set; }
        /// Title
        public string Title { get; set; } = string.Empty;
        /// Description
        public string Description { get; set; } = string.Empty;
        /// StartAt
        public DateTime? StartAt { get; set; }
        /// EndAt
        public DateTime? EndAt { get; set; }
        [Required]
        public int TotalSeats { get; set; }

        private int _AvailableSeats;
        public int AvailableSeats
        {
            get => _AvailableSeats;
            set { _AvailableSeats = value; }
        }
        public Event(int totalSeats)
        {
            _AvailableSeats = totalSeats;
        }
        public bool TryReserveSeats(int count = 1)
        {
            if ((_AvailableSeats - count) < 0)
                return false;
            else
            {
                _AvailableSeats -= count;
                return true;
            }
        }
        public void ReleaseSeats(int count = 1)
        {
            _AvailableSeats += count;
        }
    }
}
