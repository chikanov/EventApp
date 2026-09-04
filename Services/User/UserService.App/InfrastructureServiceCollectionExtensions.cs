using UserService.Application.Abstractions.Persistence.Repositories;
using UserService.Infrastructure.Persistence.DataAccess;
using UserService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace UserService.App
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            return builder.Services;
        }

        public static void DatabaseMigrate(WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
                db.Database.Migrate();
            }
        }
    }
}
