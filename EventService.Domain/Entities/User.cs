using EventService.Domain.CustomExceptions;
using EventService.Domain.Entities.Enum;

namespace EventService.Domain.Entities
{
    public class User
    {
        // Id
        public Guid Id { get; set; }
        // Login
        public string Login { get; set; }
        // Password
        public string Password { get; set; }
        // Role
        public UserRoles Role { get; set; }
        public ICollection<Booking> Bookings { get; set; } = [];

        public User(Guid id, string login, string password, UserRoles role)
        {
            Id = id;
            Login = login;
            Password = password;
            Role = role;
        }
        public void Update(
            string login,
            string password,
            UserRoles role)
        {
            Login = login;
            Password = password;
            Role = role;
        }

        public static User CreateUser(
            string login,
            string password,
            UserRoles role)
        {
            return new User(Guid.NewGuid(), login, password, role);
        }

        private static void AddError(Dictionary<string, ICollection<string>> errors, string field, string message)
        {
            if (!errors.ContainsKey(field))
                errors[field] = new List<string>();

            errors[field].Add(message);
        }

        public static void ValidateUser(User user)
        {
            var errors = new Dictionary<string, ICollection<string>>();

            if (string.IsNullOrWhiteSpace(user.Login))
                AddError(errors, nameof(user.Login), "Login cannot be empty.");
            if (string.IsNullOrWhiteSpace(user.Password))
                AddError(errors, nameof(user.Password), "Password cannot be empty.");

            if (errors.Any())
                throw new ValidationException(errors);
        }
    }
}
