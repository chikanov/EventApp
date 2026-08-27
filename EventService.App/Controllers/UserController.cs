using EventService.Application.Abstractions.Services;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using EventService.Domain.Entities.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventService.App.Controllers
{
    /// UsersController
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        /// text
        public UsersController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }
        [AllowAnonymous]
        [HttpPost("auth/login")]
        public async Task<ActionResult> Login([FromQuery] string login, [FromQuery] string password, CancellationToken cancellationToken)
        {
            try
            {
                var currentUser = await _userService.GetByLogin(login);
                if (currentUser == null)
                {
                    return Unauthorized();
                }
                if (!AuthenticationComponent.VerifyPassword(password, currentUser.Password))
                {
                    return BadRequest();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, login),
                    new Claim("Role", currentUser.Role.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, currentUser.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var authenticationParams = AuthenticationComponent.GetAuthenticationParams(_configuration);

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationParams["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: authenticationParams["ValidIssuer"],
                    audience: authenticationParams["ValidAudience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(int.Parse(authenticationParams["TokenLifeTimeMinutes"])),
                    signingCredentials: creds
                );

                string accessToken = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new { Token = accessToken });
            }
            catch (Exception ex)
            {
                return new UnauthorizedResult();
            }
        }

        /// <summary>
        /// GET: Get All Users.
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="role">User role</param>
        /// <returns>Collection Users</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("api/[controller]")]
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
        [Authorize(Roles = "Admin")]
        [HttpGet("api/[controller]/{id}")]
        [ActionName("GetUserByIdAsync")]
        public async Task<ActionResult<User>> GetUserByIdAsync([FromRoute] Guid id)
        {
            var user = await _userService.GetByIdAsync(id);

            return Ok(user);
        }

        /// <summary>
        /// POST: Create new user.
        /// </summary>
        /// <returns>User user</returns>
        [AllowAnonymous]
        [HttpPost("auth/register")]
        public async Task<ActionResult<User>> CreateUserAsync(UserDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            user.Password = AuthenticationComponent.HashPassword(user.Password);
            var createdUser = await _userService.CreateUserAsync(user);

            return NoContent();
        }

        /// <summary>
        /// PUT: Update User
        /// </summary>
        /// <returns>User user</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("api/[controller]/{id}")]
        public async Task<ActionResult<UserDto>> UpdateUserAsync([FromRoute] Guid id, UserDto user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            user.Password = AuthenticationComponent.HashPassword(user.Password);
            var updatedEvent = await _userService.UpdateUserAsync(id, user);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// DELETE: Delete User
        /// </summary>
        /// <returns>User user</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("api/[controller]/{id}")]
        public async Task<ActionResult<User>> DeleteUserAsync([FromRoute] Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
