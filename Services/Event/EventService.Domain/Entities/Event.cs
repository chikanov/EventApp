using EventService.Domain.CustomExceptions;

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
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        private Event() { Title = null!; }

        private Event(
            string title,
            string? description,
            DateTime startAt,
            DateTime endAt,
            int totalSeats
            )
        {
            Title = title;
            StartAt = startAt;
            EndAt = endAt;
            TotalSeats = totalSeats;
            AvailableSeats = totalSeats;
            Description = description!;
        }
        public static Event Create(
            string? title,
            string? description,
            DateTime? startAt,
            DateTime? endAt,
            int? totalSeats = null
        )
        {
            return new Event(title!.Trim(), description, startAt!.Value, endAt!.Value, totalSeats!.Value);
        }
        public void Update(
            string? title,
            string? description,
            DateTime? startAt,
            DateTime? endAt,
            int totalSeats,
            int availableSeats)
        {
            if (availableSeats < 0 || availableSeats > totalSeats)
                new ValidationEventException(nameof(availableSeats),
                    "The AvailableSeats parameter must not be less than 0 or greater than the TotalSeats parameter.");
            Title = title!;
            Description = description!;
            StartAt = startAt!.Value;
            EndAt = endAt!.Value;
            TotalSeats = totalSeats;
            AvailableSeats = availableSeats;
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
