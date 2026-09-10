using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.DataAccess.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("events");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();

            builder.Property(e => e.Title).IsRequired().HasColumnName("title").HasMaxLength(50);

            builder.Property(e => e.Description).IsRequired().HasColumnName("description").HasMaxLength(1000);

            builder.Property(b => b.StartAt).HasColumnName("start_at").IsRequired();

            builder.Property(b => b.EndAt).HasColumnName("end_at").IsRequired();

            builder.Property(b => b.TotalSeats).HasColumnName("total_seats").IsRequired();

            builder.Property(e => e.AvailableSeats).HasColumnName("available_seats").IsRequired();

        }
    }
}
