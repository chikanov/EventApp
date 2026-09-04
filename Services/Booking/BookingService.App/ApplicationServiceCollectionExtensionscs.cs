using BookingService.Application.Abstractions.Services;
using BookingService.Application.BackgroundServices;
using BookingService.Infrastructure.Persistence.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BookingService.App
{
    public static class ApplicationServiceCollectionExtensionscs
    {
        public static IServiceCollection AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<BookingDbContext>(options =>
                options.UseNpgsql(connectionString));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped<IBookingService, BookingService.Application.Services.BookingService>();

            builder.Services.AddHostedService<BookingBackgroundService>();

            return builder.Services;
        }
    }
}
