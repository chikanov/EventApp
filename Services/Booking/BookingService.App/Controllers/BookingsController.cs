using BookingService.Application.Abstractions.Services;
using BookingService.Domain.CustomExceptions;
using BookingService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BookingService.App.Controllers
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
        /// POST: Create new booking.
        /// </summary>
        /// <param name="id">Event Id</param>
        /// <param name="token">CancellationToken</param>
        /// <returns>Return Booking and link to booking in Headers</returns>
        [Authorize(Roles = "User, Admin")]
        [HttpPost]
        [Route("{id}/book")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Booking>> CreateBookingAsync([FromRoute] int id, CancellationToken token)
        {
            var userId = GetUserId();

            try
            {
                var newBooking = await _bookingService.CreateBookingAsync(id, userId, token);

                return Accepted($"/bookings/{newBooking.Id}", newBooking);
            }
            catch (PastEventBookingException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ActiveLeasesExceededException ex)
            {
                return Conflict(ex.Message);
            }
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
        [HttpDelete("{id}")]
        public async Task<ActionResult<Booking>> CancellationBooking([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetUserId();

                var cancellationBookig = await _bookingService.CancellationBookingAsync(id, userId, cancellationToken);
                return NoContent();
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
