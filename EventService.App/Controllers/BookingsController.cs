using EventService.Application.Abstractions.Services;
using EventService.Domain.CustomExceptions;
using EventService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventService.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingsController(IBookingService bookingService, IHttpContextAccessor httpContextAccessor)
        {
            _bookingService = bookingService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// GET: Get Booking by id.
        /// </summary>
        /// <param name="id">Booking guid Id</param>
        /// <returns>Return Booking</returns>
        [Authorize(Roles = "User,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookingByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var booking = await _bookingService.GetBookingByIdAsync(id, userId, cancellationToken);

            return Ok(booking);
        }
        [Authorize(Roles = "User,Admin")]
        [HttpPost("{id}")]
        public async Task<ActionResult<Booking>> CancellationBooking([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserId();

                var cancellationBookig = await _bookingService.CancellationBookingAsync(id, userId, cancellationToken);
                return Ok(cancellationBookig);
            }
            catch (PermissionDeniedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private Guid GetUserId()
        {
            var currentUser = _httpContextAccessor?.HttpContext?.User;
            var userIdClaim = currentUser!.FindFirst(ClaimTypes.NameIdentifier);
            var userId = Guid.Parse(userIdClaim!.Value);
            return userId;
        }
    }
}
