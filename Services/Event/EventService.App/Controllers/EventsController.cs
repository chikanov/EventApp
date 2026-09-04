using EventService.Application.Abstractions.Services;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.App.Controllers
{
    /// EventsController
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        /// text
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
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
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<PaginatedResult>> GetAllEventsAsync([FromQuery] string? title = null,
            [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
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
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ActionName("GetEventByIdAsync")]
        public async Task<ActionResult<Event>> GetEventByIdAsync([FromRoute] int id)
        {
            var ev = await _eventService.GetByIdAsync(id);

            return Ok(ev);
        }

        /// <summary>
        /// POST: Create new event.
        /// </summary>
        /// <returns>Event eventt</returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEventAsync(CreateEventDto ev)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdEvent = await _eventService.CreateEventAsync(ev);

            return CreatedAtAction(nameof(GetEventByIdAsync), new { id = createdEvent.Id }, createdEvent);
        }

        /// <summary>
        /// PUT: Update Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<ActionResult<EventDto>> UpdateEventAsync([FromRoute] int id, EventDto ev)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedEvent = await _eventService.UpdateEventAsync(id, ev);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// DELETE: Delete Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Event>> DeleteEventAsync([FromRoute] int id)
        {
            await _eventService.DeleteEventAsync(id);
            return NoContent();
        }
    }
}
