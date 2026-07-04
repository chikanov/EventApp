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
        Task<PaginatedResult> GetAllAsync(int page, int pageSize, string? title, DateTime? From, DateTime? to, CancellationToken cancellationToken = default);
        /// Event? GetById
        Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        /// Event Add
        Task<Event> CreateEventAsync(CreateEventDto ev, CancellationToken cancellationToken = default);
        /// Event Update
        Task<Event> UpdateEventAsync(int id, EventDto ev, CancellationToken cancellationToken = default);
        /// Event Delete
        Task<Event> DeleteEventAsync(int id, CancellationToken cancellationToken = default);
        Task<Event> UpdateEventAsync(int id, Event ev, CancellationToken cancellationToken = default);

    }
}
