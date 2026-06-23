using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.DTO;
using System.ComponentModel.DataAnnotations;

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
            new Event(100){ Id = 1, Title = "Title1", Description = "Description1", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1), TotalSeats = 100},
            new Event(100){ Id = 2, Title = "Title2", Description = "Description2", StartAt = DateTime.Now, EndAt = DateTime.Now.AddDays(1), TotalSeats = 100},
            new Event(100){ Id = 3, Title = "Title3", Description = "Description3", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2), TotalSeats = 100},
            new Event(100){ Id = 4, Title = "Title4", Description = "Description4", StartAt = DateTime.Now.AddDays(1), EndAt = DateTime.Now.AddDays(2), TotalSeats = 100},
            new Event(100){ Id = 5, Title = "Title5", Description = "Description5", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3), TotalSeats = 100},
            new Event(100){ Id = 6, Title = "Title6", Description = "Description6", StartAt = DateTime.Now.AddDays(2), EndAt = DateTime.Now.AddDays(3), TotalSeats = 100},
            new Event(100){ Id = 7, Title = "Title7", Description = "Description7", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4), TotalSeats = 100},
            new Event(100){ Id = 8, Title = "Title8", Description = "Description8", StartAt = DateTime.Now.AddDays(3), EndAt = DateTime.Now.AddDays(4), TotalSeats = 100},
            new Event(100){ Id = 9, Title = "Title9", Description = "Description9", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5), TotalSeats = 100},
            new Event(100){ Id = 10, Title = "Title10", Description = "Description10", StartAt = DateTime.Now.AddDays(4), EndAt = DateTime.Now.AddDays(5), TotalSeats = 100},
            new Event(100){ Id = 11, Title = "Title11", Description = "Description11", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7), TotalSeats = 100},
            new Event(100){ Id = 12, Title = "Title12", Description = "Description12", StartAt = DateTime.Now.AddDays(6), EndAt = DateTime.Now.AddDays(7), TotalSeats = 100},
            new Event(100){ Id = 13, Title = "Title13", Description = "Description13", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9), TotalSeats = 100},
            new Event(100){ Id = 14, Title = "Title14", Description = "Description14", StartAt = DateTime.Now.AddDays(8), EndAt = DateTime.Now.AddDays(9), TotalSeats = 100},
            new Event(100){ Id = 15, Title = "Title15", Description = "Description15", StartAt = DateTime.Now.AddDays(9), EndAt = DateTime.Now.AddDays(10), TotalSeats = 100}
        };

        ///GetAll() 
        public List<Event> GetAll()
        {
            return _events;
        }
        ///GetAll() 
        public PaginatedResult GetAll(int page, int pageSize, string? Title = null, 
            DateTime? from = null, DateTime? to = null)
        {
            return GetEventList(_events, Title, from, to, page, pageSize);
        }

        ///GetById
        public Event? GetById(int id)
        {
            return _events.FirstOrDefault(e => e.Id == id);
        }

        /// Add
        public Event Add(CreateEventDto ev)
        {
            if (ev.TotalSeats <= 0)
            {
                throw new ValidationException("Total seats value must be greater than zero.");
            }
            var newEventId = _events.Any() ? _events.Max(e => e.Id) + 1 : 1;
            Event newEvent = new Event(ev.TotalSeats) 
            {
                Id = newEventId,
                Title = ev.Title,
                Description = ev.Description,
                StartAt = ev.StartAt,
                EndAt = ev.EndAt,
                TotalSeats = ev.TotalSeats,
            };
            _events.Add(newEvent);
            return newEvent;
        }

        /// Update
        public Event Update(int id, EventDto ev)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == id);

            if (existEvent == null)
            {
                throw new NotFoundException($"Event with Id = {id} does not exist.");
            }

            if (ev.StartAt > ev.EndAt)
            {
                throw new ValidationException("The end date must be greater than the start date.");
            }

            if (ev.TotalSeats <= 0)
            {
                throw new ValidationException("Total seats value must be greater than zero.");
            }

            if (existEvent != null)
            {
                existEvent.Id = id;
                existEvent.Title = ev.Title;
                existEvent.Description = ev.Description;
                existEvent.StartAt = ev.StartAt;
                existEvent.EndAt = ev.EndAt;
                existEvent.TotalSeats = ev.TotalSeats;
            }
            return existEvent;
        }

        public Event Update(int id)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == id);

            if (existEvent == null)
            {
                throw new NotFoundException($"Event with Id = {id} does not exist.");
            }

            if (existEvent.StartAt > existEvent.EndAt)
            {
                throw new ValidationException("The end date must be greater than the start date.");
            }

            if (existEvent.TotalSeats <= 0)
            {
                throw new ValidationException("Total seats value must be greater than zero.");
            }

            if (existEvent != null)
            {
                existEvent.Id = id;
                existEvent.Title = existEvent.Title;
                existEvent.Description = existEvent.Description;
                existEvent.StartAt = existEvent.StartAt;
                existEvent.EndAt = existEvent.EndAt;
                existEvent.TotalSeats = existEvent.TotalSeats;
            }
            return existEvent;
        }

        /// Delete
        public Event Delete(int id)
        {
            var existEvent = _events.FirstOrDefault(e => e.Id == id);
            if (existEvent == null)
            {
                throw new NotFoundException($"Event with Id = {id} does not exist.");
            }
            if (existEvent != null)
                _events.Remove(existEvent);

            return existEvent;
        }
        public PaginatedResult GetEventList(
            IEnumerable<Event> events,
            string? title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize)
        {
            if (!string.IsNullOrEmpty(title))
            {
                events = events.Where(e => e.Title.Contains(title ?? "", StringComparison.OrdinalIgnoreCase));
            }

            if (from != null)
            {
                events = events.Where(e => e.StartAt >= GetTheStartOfTheDayOrDefault(from));
            }

            if (to != null)
            {
                events = events.Where(e => e.EndAt <= GetTheEndOfTheDayOrDefault(to));
            }

            int filteredCount = events.Count();

            var items = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            int totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);

            return new PaginatedResult() { 
                CountEventsOnPage = items.Count,
                ListEvents = items, 
                EventsCount = filteredCount, 
                Page = page
            };
        }

        DateTime? GetTheStartOfTheDayOrDefault(DateTime? from)
        {
            return from.Value.AddHours(0 - from.Value.Hour).
                AddMinutes(0 - from.Value.Minute).
                AddSeconds(0 - from.Value.Second);
        }

        DateTime? GetTheEndOfTheDayOrDefault(DateTime? to)
        {
            return to!.Value.AddHours(23 - to.Value.Hour).
                AddMinutes(59 - to.Value.Minute).
                AddSeconds(59 - to.Value.Second);
        }
    }
}
