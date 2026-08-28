using EventService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EventService.Infrastructure.Persistence.DataAccess.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
            
            builder.Property(e => e.Login).IsRequired().HasColumnName("login").HasMaxLength(50);
            builder.Property(e => e.Password).IsRequired().HasColumnName("password").HasMaxLength(100);
            builder.Property(b => b.Role).HasColumnName("role").IsRequired().HasMaxLength(20).HasConversion<string>();

            builder.HasMany(b => b.Bookings).WithOne(b => b.User).HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(e => e.Login).IsUnique();
        }
    }
}
