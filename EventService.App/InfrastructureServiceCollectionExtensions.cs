using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Infrastructure.Persistence.DataAccess;
using EventService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(connectionString));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped<IEventRepository, EventRepository>();
            builder.Services.AddScoped<IBookingRepository, BookingRepository>();

            return builder.Services;
        }

        public static void DatabaseMigrate(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
            }
        }
    }
}
