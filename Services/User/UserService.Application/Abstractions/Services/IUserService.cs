using UserService.Application.DTOs;
using UserService.Domain.Entities;
using UserService.Domain.Entities.Enum;

namespace UserService.Application.Abstractions.Services
{
    public interface IUserService
    {
        /// Filtred collection Users GetAll
        Task<List<User>> GetAllAsync(string? login, UserRoles? role, CancellationToken cancellationToken = default);
        /// User GetById
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        /// User Add
        Task<User> CreateUserAsync(UserDto user, CancellationToken cancellationToken = default);
        /// User Update
        Task<User> UpdateUserAsync(Guid id, UserDto user, CancellationToken cancellationToken = default);
        /// User Delete
        Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
        /// User GetByLogin
        Task<User> GetByLogin(string login, CancellationToken cancellationToken = default);
    }
}
