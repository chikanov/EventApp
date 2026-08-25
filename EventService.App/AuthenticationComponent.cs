using EventService.Domain.Entities.Enum;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EventService.App
{
    public static class AuthenticationComponent
    {

        public static IServiceCollection AddAuthentication(this WebApplicationBuilder builder)
        {
            var ValidIssuer = builder.Configuration.GetValue<string>("TokenValidationParameters: ValidIssuer")
                ?? throw new InvalidOperationException("TokenValidationParameters 'ValidIssuer' not found.");
            var ValidAudience = builder.Configuration.GetValue<string>("TokenValidationParameters: ValidAudience")
                            ?? throw new InvalidOperationException("TokenValidationParameters 'ValidAudience' not found.");
            var SecretKey = builder.Configuration.GetValue<string>("TokenValidationParameters: SecretKey")
                            ?? throw new InvalidOperationException("TokenValidationParameters 'SecretKey' not found.");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearerScheme";
                options.DefaultChallengeScheme = "JwtBearerScheme";
            })
            .AddJwtBearer("JwtBearerScheme", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = "role",
                    NameClaimType = "login",
                    ValidateIssuer = true,
                    ValidIssuer = ValidIssuer,

                    ValidateAudience = true,
                    ValidAudience = ValidAudience,

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(SecretKey))
                };
            });
            return builder.Services;
        }

        public static string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password), "The password cannot be null.");

            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordBytes = Encoding.UTF8.GetBytes(password);
                var hashBytes = sha256.ComputeHash(passwordBytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            if (password == null)
                return false;

            storedHash = HashPassword(password); 
            return storedHash.Equals(storedHash);
        }
    }
}
