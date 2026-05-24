using EventApp.Services;

namespace EventApp
{
    public class EventServiceFixture
    {
        public EventService eventService {  get; set; }

        public EventServiceFixture()
        {
            eventService = new EventService();
        }
    }
}
