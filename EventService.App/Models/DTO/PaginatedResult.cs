namespace EventApp.Models.DTO
{
    public class PaginatedResult
    {
        public int EventsCount { get; set; }
        public List<Event> ListEvents { get; set; }
        public int Page { get; set; }
        public int CountEventsOnPage { get; set; }  
    }
}
