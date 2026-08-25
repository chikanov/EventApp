using EventService.Application.Abstractions.Services;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using EventService.Domain.Entities.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventService.App.Controllers
{
    /// UsersController
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// text
        public UsersController(IUserService userService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] string login, [FromBody] string password, CancellationToken cancellationToken)
        {
            var currentUser = await _userService.GetByLogin(login);
            if (currentUser == null || AuthenticationComponent.VerifyPassword(password, currentUser.Password))
            {
                return new UnauthorizedResult();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, login),
                new Claim(ClaimTypes.Role, currentUser.Role.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, currentUser.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var secretKey = _configuration.GetValue<string>("TokenValidationParameters:SecretKey");
            var validIssuer = _configuration.GetValue<string>("TokenValidationParameters:ValidIssuer");
            var validAudience = _configuration.GetValue<string>("TokenValidationParameters:ValidAudience");
            var TokenLifeTimeMinutes = _configuration.GetValue<int>("TokenValidationParameters:TokenLifeTimeMinutes");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: validIssuer,
                audience: validAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(TokenLifeTimeMinutes),
                signingCredentials: creds
            );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { Token = accessToken });
        }

        /// <summary>
        /// GET: Get All Users.
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="role">User role</param>
        /// <returns>Collection Users</returns>
        [HttpGet]
        public async Task<ActionResult<List<User>?>> GetAllUsersAsync([FromQuery] string? login = null,
            [FromQuery] UserRoles? role = null)
        {
            var result = await _userService.GetAllAsync(login, role);

            return Ok(result);
        }

        /// <summary>
        /// GET: Get User by id.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>User user</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserByIdAsync([FromRoute] Guid id)
        {
            var user = await _userService.GetByIdAsync(id);

            return Ok(user);
        }

        /// <summary>
        /// POST: Create new user.
        /// </summary>
        /// <returns>User user</returns>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUserAsync(UserDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            user.Password = AuthenticationComponent.HashPassword(user.Password);
            var createdUser = await _userService.CreateUserAsync(user);

            return CreatedAtAction(nameof(GetUserByIdAsync), new { id = createdUser.Id }, createdUser);
        }

        /// <summary>
        /// PUT: Update User
        /// </summary>
        /// <returns>User user</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateEventAsync([FromRoute] Guid id, UserDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedEvent = await _userService.UpdateUserAsync(id, user);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// DELETE: Delete User
        /// </summary>
        /// <returns>User user</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteEventAsync([FromRoute] Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
