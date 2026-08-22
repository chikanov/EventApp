using System;
using System.Collections.Generic;
using System.Text;

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
        public string Role { get; set; }
        public ICollection<Booking> Bookings { get; set; } = [];
    }
}
