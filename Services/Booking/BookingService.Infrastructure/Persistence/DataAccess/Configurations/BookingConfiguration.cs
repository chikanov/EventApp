using BookingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookingService.Infrastructure.Persistence.DataAccess.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("bookings");

            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).HasColumnName("id").ValueGeneratedNever();

            builder.Property(b => b.EventId).HasColumnName("event_id").IsRequired();
            builder.Property(b => b.UserId).HasColumnName("user_id").IsRequired();

            builder.Property(b => b.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasConversion<string>();

            builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();

            builder.Property(b => b.ProcessedAt).HasColumnName("processed_at");
        }
    }
}
