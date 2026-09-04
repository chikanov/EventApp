using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Application.DTOs;
using UserService.Domain.CustomExceptions;
using UserService.Domain.Entities;
using UserService.Domain.Entities.Enum;

namespace UserService.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }
        public async Task<User> CreateUserAsync(UserDto user, CancellationToken cancellationToken = default)
        {
            var newUser = User.CreateUser(user.Login, user.Password, user.Role);
            User.ValidateUser(newUser);
            await _userRepo.AddAsync(newUser);
            return newUser;
        }

        public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existUser = await _userRepo.GetByIdAsync(id);
            if (existUser == null)
            {
                throw new NotFoundException($"User with Id = {id} does not exist.");
            }
            await _userRepo.DeleteAsync(existUser);
            return true;
        }

        public async Task<List<User>> GetAllAsync(string? login, UserRoles? role, CancellationToken cancellationToken = default)
        {
            var users = await _userRepo.GetAllAsync();

            if (!string.IsNullOrEmpty(login))
            {
                users = users.Where(e => e.Login.Contains(login ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            if (!string.IsNullOrEmpty(role.ToString()))
            {
                users = users.Where(e => e.Role.ToString().Contains(role.ToString() ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return users;
        }

        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepo.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException($"User with Id - {id} not found.");
            return user;
        }

        public async Task<User> GetByLogin(string login, CancellationToken cancellationToken = default)
        {
            return await _userRepo.GetByLogin(login, cancellationToken);
        }

        public async Task<User> UpdateUserAsync(Guid id, UserDto user, CancellationToken cancellationToken = default)
        {
            var existUser = await _userRepo.GetByIdAsync(id);
            if (existUser == null)
            {
                throw new NotFoundException($"User with Id = {id} does not exist.");
            }
            User.ValidateUser(existUser!);

            var updatedUser = await _userRepo.UpdateAsync(user, existUser!, cancellationToken);

            return updatedUser!;
        }
    }
}
