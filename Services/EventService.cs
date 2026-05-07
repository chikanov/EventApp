using EventApp.Interfaces;
using EventApp.Models;
using System.ComponentModel.Design;

namespace EventApp.Services
{
    public class EventService : IEventService
    {
        public static List<Event> _events = new() 
        {
            new Event(){ Id = 1, Title = "Tittle1", Description = "Description1", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)},
            new Event(){ Id = 2, Title = "Tittle2", Description = "Description2", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)},
            new Event(){ Id = 3, Title = "Tittle3", Description = "Description3", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)}
        };

        public List<Event> GetAll() 
        {
            return _events;
        }

        public Event? GetById(int id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        public Event Add(Event ev)
        {
            ev.Id = _events.Any() ? _events.Max(e => e.Id) + 1 : 1;
            _events.Add(ev);
            return ev;
        }

        public Event Update(Event ev)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == ev.Id);

            if (existEvent != null)
            {
                existEvent.Id = ev.Id;
                existEvent.Title = ev.Title;
                existEvent.StartAt = ev.StartAt;
                existEvent.EndAt = ev.EndAt;
            }
            return existEvent;
        }

        public Event Delete(int id)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == id);

            if(existEvent != null)
                _events.Remove(existEvent);

            return existEvent;
        }
    }
}
