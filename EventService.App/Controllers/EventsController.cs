using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace EventApp.Controllers
{
    /// EventsController
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IConfiguration _config;
        /// text
        public EventsController(IEventService eventService, IConfiguration config)
        {
            _eventService = eventService;
            _config = config;
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
        public ActionResult<PaginatedResult> GetAllEvents([FromQuery] string? title = null,
            [FromQuery]  DateTime? from = null, [FromQuery]  DateTime? to = null, 
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = _eventService.GetAll(page, pageSize, title, from, to);
            
            if (result != null)
            {
                return Ok(result);
            }

            else return NotFound();
        }

        /// <summary>
        /// GET: Get Event by id.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Event event</returns>
        [HttpGet("{id}")]
        public ActionResult<Event> GetEventById([FromRoute] int id)
        {
            var ev = _eventService.GetById(id);
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
        public ActionResult<EventDto> CreateEvent(EventDto ev)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdEvent = _eventService.Add(ev);

            return Created();
        }

        /// <summary>
        /// PUT: Update Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpPut("{id}")]
        public ActionResult<EventDto> UpdateEvent([FromRoute] int id, EventDto ev)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (_eventService.GetById(id) == null)
                return NotFound(ev);

            var updatedEvent = _eventService.Update(id, ev);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// DELETE: Delete Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpDelete("{id}")]
        public ActionResult<Event> DeleteEvent([FromRoute] int id)
        {
            if (_eventService.GetById(id) == null)
                return NotFound();

            var deletedEvent = _eventService.Delete(id);
            return NoContent();
        }
    }
}
