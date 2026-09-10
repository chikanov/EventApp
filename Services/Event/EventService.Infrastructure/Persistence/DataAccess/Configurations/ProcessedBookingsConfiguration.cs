using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.DataAccess.Configurations
{
    public class ProcessedBookingsConfiguration : IEntityTypeConfiguration<ProcessedBookings>
    {
        public void Configure(EntityTypeBuilder<ProcessedBookings> builder)
        {
            builder.ToTable("processedbookings");
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).HasColumnName("id").ValueGeneratedNever();
            builder.Property(b => b.ProcessedDateTime).HasColumnName("created_at").IsRequired();
        }
    }
}
