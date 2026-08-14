using EventService.Application.Abstractions.Services;
using EventService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventService.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// GET: Get Booking by id.
        /// </summary>
        /// <param name="id">Booking guid Id</param>
        /// <returns>Return Booking</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookingByIdAsync([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id, cancellationToken);

            return Ok(booking);
        }
    }
}
