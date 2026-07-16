using EventApp.DataAccess;
using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.Models.Enum;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;
        public BookingRepository(AppDbContext contex)
        {
            _context = contex;
        }
        public async Task AddAsync(Booking book, CancellationToken ct = default)
        {
            await _context.Bookings.AddAsync(book, ct).AsTask();
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<Booking>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Bookings.AsNoTrackingWithIdentityResolution().Include(e => e.Event).ToListAsync(ct);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Bookings.Include(b => b.Event).FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<List<Booking>> GetPendingAsync(CancellationToken ct = default)
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Pending)
                .ToListAsync(ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

        public async Task<bool> AnyAsync(CancellationToken ct = default)
        {
            return await _context.Bookings.AnyAsync(ct);
        }
    }
}
