using UserService.Application.Abstractions.Services;
using UserService.Infrastructure.Persistence.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace UserService.App
{
    public static class ApplicationServiceCollectionExtensionscs
    {
        public static IServiceCollection AddInfrastructureServices(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddOpenApi();
            builder.Services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(connectionString));
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            builder.Services.AddScoped<IUserService, UserService.Application.Services.UserService>();

            return builder.Services;
        }
    }
}
