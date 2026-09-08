using EventService.Domain.Entities;
using EventService.Infrastructure.Persistence.DataAccess.Configurations;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.Persistence.DataAccess
{
    public class EventDbContext : DbContext
    {
        public EventDbContext(DbContextOptions<EventDbContext> options) : base(options) { }

        public DbSet<Event> Events => Set<Event>();
        public DbSet<ProcessedBookings> ProcessedBookings => Set<ProcessedBookings>();       

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EventDbContext).Assembly);
        }
    }
}
