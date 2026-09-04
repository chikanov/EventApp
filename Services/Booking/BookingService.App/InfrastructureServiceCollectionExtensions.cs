using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Infrastructure.Persistence.DataAccess;
using BookingService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BookingService.App
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(connectionString));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped<IBookingRepository, BookingRepository>();

            return builder.Services;
        }

        public static void DatabaseMigrate(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
                db.Database.Migrate();
            }
        }
    }
}
