using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService, IConfiguration config)
        {
            _bookingService = bookingService;
        }
        
        /// <summary>
        /// GET: Get Booking by id.
        /// </summary>
        /// <param name="id">Booking guid Id</param>
        /// <returns>Return Booking</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBookingByIdAsync([FromRoute] Guid id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }
    }
}
