using EventApp.Interfaces;

namespace EventApp.Services
{
    public class BookingServiceFixture
    {
        public BookingService bookingService { get; set; }
        private readonly IEventService _eventService;

        public BookingServiceFixture()
        {
            _eventService = new EventService();
            bookingService = new BookingService(_eventService);
        }
    }
}
