using EventService.Application.Abstractions.Services;
using EventService.Infrastructure.Persistence.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace EventService.App
{
    public static class ApplicationServiceCollectionExtensionscs
    {
        public static IServiceCollection AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<EventDbContext>(options =>
                options.UseNpgsql(connectionString));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped<IEventService, EventService.Application.Services.EventService>();

            return builder.Services;
        }
    }
}
