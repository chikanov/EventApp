namespace EventService.Domain.Entities
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
        public int TotalSeats { get;  set; }
        public int AvailableSeats { get; set; }
        public ICollection<Booking> Bookings { get; set; } = [];
        private Event() { Title = null!; }

        private Event(
            int id,
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats
            )
        {
            Id = id;
            Title = title;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
            Description = description;
        }
        public static Event Create(
            int id,
            string? title,
            string? description,
            DateTime? startAt,
            DateTime? endAt,
            int? totalSeats = null
        )
        {
            return new Event(id, title!.Trim(), description, startAt!.Value, endAt!.Value, totalSeats!.Value);
        }
        public void Update(
            string? title,
            string? description,
            DateTime? startAt,
            DateTime? endAt,
            int totalSeats,
            int availebleSeats)
        {
            Title = title!;
            Description = description!;
            StartAt = startAt!.Value;
            EndAt = endAt!.Value;
            TotalSeats = totalSeats;
            AvailableSeats = availebleSeats;
        }
        public bool TryReserveSeats(int count = 1)
        {
            if ((AvailableSeats - count) < 0)
                return false;
            else
            {
                AvailableSeats -= count;
                return true;
            }
        }
        public void ReleaseSeats(int count = 1)
        {
            AvailableSeats += count;
        }
    }
}
