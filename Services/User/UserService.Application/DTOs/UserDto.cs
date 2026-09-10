using UserService.Domain.Entities.Enum;
using System.ComponentModel.DataAnnotations;

namespace UserService.Application.DTOs
{
    public class UserDto
    {
        // Login
        [Required]
        [StringLength(20, MinimumLength = 3,
            ErrorMessage = "The login should be between 3 and 20 characters long.")]
        public string Login { get; set; }
        // Password
        [Required]
        [StringLength(30, MinimumLength = 8,
            ErrorMessage = "The password should be between 8 and 30 characters long.")]
        public string Password { get; set; }
        // Role
        [Required]
        public UserRoles Role { get; set; }
    }
}
