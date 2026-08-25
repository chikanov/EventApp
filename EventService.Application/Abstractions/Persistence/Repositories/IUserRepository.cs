using EventService.Application.DTOs;
using EventService.Domain.Entities;

namespace EventService.Application.Abstractions.Persistence.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<User>> GetAllAsync(CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        Task<User?> UpdateAsync(UserDto dto, User user, CancellationToken ct = default);
        Task DeleteAsync(User user, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<User?> GetByLogin(string login, CancellationToken ct = default);
    }
}
