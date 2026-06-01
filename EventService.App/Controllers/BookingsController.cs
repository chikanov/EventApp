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
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly IConfiguration _config;

        public BookingsController(IEventService eventService, IBookingService bookingService, IConfiguration config)
        {
            _eventService = eventService;
            _bookingService = bookingService;
            _config = config;
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
