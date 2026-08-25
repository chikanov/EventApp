using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace EventService.App
{
    public static class AuthenticationComponent
    {
        public static IServiceCollection AddAuthentication(this WebApplicationBuilder builder)
        {
            var authenticationParams = GetAuthenticationParams(builder.Configuration);
            
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
                    ValidIssuer = authenticationParams["ValidIssuer"],

                    ValidateAudience = true,
                    ValidAudience = authenticationParams["ValidAudience"],

                    ValidateLifetime = true,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(authenticationParams["SecretKey"]))
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

        public static IReadOnlyDictionary<string, string> GetAuthenticationParams(IConfiguration configuration)
        {
            var ValidIssuer = configuration.GetValue<string>("TokenValidationParameters: ValidIssuer")
                ?? throw new InvalidOperationException("TokenValidationParameters 'ValidIssuer' not found.");
            var ValidAudience = configuration.GetValue<string>("TokenValidationParameters: ValidAudience")
                ?? throw new InvalidOperationException("TokenValidationParameters 'ValidAudience' not found.");
            var SecretKey = configuration.GetValue<string>("TokenValidationParameters: SecretKey")
                ?? throw new InvalidOperationException("TokenValidationParameters 'SecretKey' not found.");
            var TokenLifeTimeMinutes = configuration.GetValue<int>("TokenValidationParameters:TokenLifeTimeMinutes");
            if (TokenLifeTimeMinutes == null)
            {
                throw new InvalidOperationException("TokenValidationParameters 'TokenLifeTimeMinutes' not found.");
            }
            return new Dictionary<string, string>
            {
                { "ValidIssuer",  ValidIssuer},
                { "ValidAudience",  ValidIssuer},
                { "SecretKey",  ValidIssuer},
                { "TokenLifeTimeMinutes",  TokenLifeTimeMinutes.ToString()},
            }.AsReadOnly();
        }
    }
}
