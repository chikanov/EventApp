using EventApp.Models;
using EventApp.Models.DTO;

namespace EventApp.Interfaces
{
    /// <summary>
    /// IEventService
    /// </summary>
    public interface IEventService
    {
        /// Filtred collection Event GetAll
        PaginatedResult GetAll(int page, int pageSize, string? title, DateTime? From, DateTime? to);
        /// Event? GetById
        Event? GetById(int id);
        /// Event Add
        Event Add(CreateEventDto ev);
        /// Event Update
        Event Update(int id, EventDto ev);
        /// Event Delete
        Event Delete(int id);
        Event Update(int id, Event ev);

    }
}
