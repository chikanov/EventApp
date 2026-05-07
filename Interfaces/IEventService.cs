using EventApp.Models;

namespace EventApp.Interfaces
{
    public interface IEventService
    {
        List<Event> GetAll();
        Event? GetById(int id);
        Event Add(Event ev);
        Event Update(Event ev);
        Event Delete(int id);

    }
}
