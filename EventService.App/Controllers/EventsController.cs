using EventService.Application.Abstractions.Services;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventService.App.Controllers
{
    /// EventsController
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        /// text
        public EventsController(IEventService eventService, IBookingService bookingService)
        {
            _eventService = eventService;
            _bookingService = bookingService;
        }

        /// <summary>
        /// GET: Get All Events.
        /// </summary>
        /// <param name="title">Event title</param>
        /// <param name="from">Date when event start</param>
        /// <param name="to">Date when event finished</param>
        /// <param name="page">Number of page</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Collection Events</returns>
        [HttpGet]
        public async Task<ActionResult<PaginatedResult>> GetAllEventsAsync([FromQuery] string? title = null,
            [FromQuery]  DateTime? from = null, [FromQuery]  DateTime? to = null, 
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _eventService.GetAllAsync(page, pageSize, title, from, to);
            return Ok(result);
        }

        /// <summary>
        /// GET: Get Event by id.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Event event</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEventByIdAsync([FromRoute] int id)
        {
            var ev = await _eventService.GetByIdAsync(id);
            if (ev == null)
            {
                return NotFound();
            }
            
            return Ok(ev);
        }

        /// <summary>
        /// POST: Create new event.
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEventAsync(CreateEventDto ev)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _eventService.CreateEventAsync(ev);

            return Created();
        }

        /// <summary>
        /// PUT: Update Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<EventDto>> UpdateEventAsync([FromRoute] int id, EventDto ev)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (await _eventService.GetByIdAsync(id) == null)
                return NotFound(ev);

            var updatedEvent = await _eventService.UpdateEventAsync(id, ev);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// DELETE: Delete Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEventAsync([FromRoute] int id)
        {
            if (await _eventService.GetByIdAsync(id) == null)
                return NotFound();

            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }

        /// <summary>
        /// POST: Create new booking.
        /// </summary>
        /// <param name="eventId">Event Id</param>
        /// <returns>Return Booking and link to booking in Headers</returns>
        [HttpPost]
        [Route("{id}/book")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Booking>> CreateBookingAsync([FromRoute]int id)
        {
            if (await _eventService.GetByIdAsync(id) == null)
            {
                return NotFound();
            }

            var newBooking = await _bookingService.CreateBookingAsync(id);

            return Accepted($"/bookings/{newBooking.Id}", newBooking);
        }
    }
}
