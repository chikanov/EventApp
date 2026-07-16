using EventApp.Models;

namespace EventApp.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<Event>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(Event @event, CancellationToken ct = default);
        Task UpdateAsync(Event @event, CancellationToken ct = default);
        Task DeleteAsync(Event @event, CancellationToken ct = default);
        Task<bool> AnyAsync(CancellationToken ct = default);
        Task<int> MaxAsync(CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
    }
}
