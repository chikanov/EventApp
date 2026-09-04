using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace UserService.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;
        public UserRepository(UserDbContext context)
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
            return await _context.Users.AsNoTrackingWithIdentityResolution().ToListAsync(ct);
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(e => e.Id == id, ct);
        }

        public async Task<User?> GetByLogin(string login, CancellationToken ct = default)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Login == login, ct);
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
