using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.DTO;

namespace EventApp.Services
{
    /// <summary>
    /// EventService
    /// </summary>
    public class EventService : IEventService
    {
        /// Collection Events
        public static List<Event> _events = new() 
        {
            new Event(){ Id = 1, Title = "Tittle1", Description = "Description1", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)},
            new Event(){ Id = 2, Title = "Tittle2", Description = "Description2", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)},
            new Event(){ Id = 3, Title = "Tittle3", Description = "Description3", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1)}
        };

        ///GetAll() 
        public List<Event> GetAll() 
        {
            return _events;
        }

        ///GetById
        public Event? GetById(int id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        /// Add
        public Event Add(EventDto ev)
        {
            var newEventId = _events.Any() ? _events.Max(e => e.Id) + 1 : 1;
            Event newEvent = new Event() 
            {
                Id = newEventId,
                Title = ev.Title,
                Description = ev.Description,
                StartAt = ev.StartAt,
                EndAt = ev.EndAt
            };
            _events.Add(newEvent);
            return newEvent;
        }

        /// Update
        public Event Update(int id, EventDto ev)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == id);

            if (existEvent != null)
            {
                existEvent.Id = id;
                existEvent.Title = ev.Title;
                existEvent.Description = ev.Description;
                existEvent.StartAt = ev.StartAt;
                existEvent.EndAt = ev.EndAt;
            }
            return existEvent;
        }

        /// Delete
        public Event Delete(int id)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == id);

            if(existEvent != null)
                _events.Remove(existEvent);

            return existEvent;
        }
    }
}
