using EventApp.Models;
using EventApp.Models.DTO;

namespace EventApp.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Event>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Event @event, CancellationToken ct = default);
        Task<Event?> UpdateAsync(EventDto dto, Event @event, CancellationToken ct = default);
        Task<Event?> UpdateAsync(Event @event, CancellationToken ct = default);
        Task DeleteAsync(Event @event, CancellationToken ct = default);
        Task<bool> AnyAsync(CancellationToken ct = default);
        Task<int> MaxAsync(CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
