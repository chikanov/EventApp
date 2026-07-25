using EventApp.DataAccess;
using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;
        public EventRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Event @event, CancellationToken ct = default)
        {
            await _context.Events.AddAsync(@event, ct).AsTask();
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(Event @event, CancellationToken ct = default)
        {
            _context.Remove(@event);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<Event>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Events.AsNoTrackingWithIdentityResolution().Include(e => e.Bookings).ToListAsync(ct);
        }

        public async Task<Event?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            return await _context.Events.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<Event?> UpdateAsync(EventDto dto, Event @event, CancellationToken ct = default)
        {
            @event.Update(dto.Title, dto.Description, dto.StartAt, dto.EndAt, dto.TotalSeats, dto.AvailableSeats);
            await _context.SaveChangesAsync(ct);
            return @event;
        }
        public async Task<Event?> UpdateAsync(Event @event, CancellationToken ct = default)
        {
            @event.Update(@event.Title, @event.Description, @event.StartAt, @event.EndAt, @event.TotalSeats, @event.AvailableSeats);
            await _context.SaveChangesAsync(ct);
            return @event;
        }

        public async Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return await _context.Events.AnyAsync(ct);
        }

        public async Task<int> MaxAsync(CancellationToken ct = default)
        {
            return await _context.Events.MaxAsync(e => e.Id, ct);
        }
        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }
    }
}
