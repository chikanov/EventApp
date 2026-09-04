using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Domain.Entities;
using BookingService.Domain.Entities.Enum;
using BookingService.Infrastructure.Persistence.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BookingService.Infrastructure.Persistence.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;
        public BookingRepository(BookingDbContext contex)
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
            return await _context.Bookings.AsNoTrackingWithIdentityResolution().ToListAsync(ct);
        }

        public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Bookings.FirstOrDefaultAsync(e => e.Id == id, ct);
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

        public async Task<List<Booking>> GetUserOwnBookingAsync(Guid userId, int eventId, CancellationToken ct = default)
        {
            return await _context.Bookings.Where(b => b.UserId == userId && b.EventId == eventId &&
                        b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed).ToListAsync(ct);
        }
    }
}
