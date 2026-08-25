using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using EventService.Infrastructure.Persistence.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventService.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            await _context.Users.AddAsync(user, ct).AsTask();
            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(User user, CancellationToken ct = default)
        {
            _context.Remove(user);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Users.AsNoTrackingWithIdentityResolution().Include(e => e.Bookings).ToListAsync(ct);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task SaveChangesAsync(CancellationToken ct = default)
        {
            await _context.SaveChangesAsync(ct);
        }

        public async Task<User?> UpdateAsync(UserDto dto, User user, CancellationToken ct = default)
        {
            user.Update(dto.Login, dto.Password, dto.Role);
            await _context.SaveChangesAsync(ct);
            return user;
        }
    }
}
