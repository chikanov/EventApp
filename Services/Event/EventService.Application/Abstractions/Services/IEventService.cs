using EventService.Application.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application.Abstractions.Services
{
    /// <summary>
    /// IEventService
    /// </summary>
    public interface IEventService
    {
        /// Filtred collection Event GetAll
        Task<PaginatedResult> GetAllAsync(int page, int pageSize, string? title = null, DateTime? From = null, DateTime? to = null, CancellationToken cancellationToken = default);
        /// Filtred read only collection Event GetAll
        Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken cancellationToken = default);
        /// Event? GetById
        Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        /// Event Add
        Task<Event> CreateEventAsync(CreateEventDto ev, CancellationToken cancellationToken = default);
        /// Event Update
        Task<Event> UpdateEventAsync(int id, EventDto ev, CancellationToken cancellationToken = default);
        /// Event Delete
        Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken = default);

    }
}
