using EventApp.Interfaces;
using EventApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace EventApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// GET: Get All Events.
        /// </summary>
        /// <returns>List<Event> AllEvents</returns>
        [HttpGet]
        public ActionResult<List<Event>> GetAllEvents()
        {
            var allEvents = _eventService.GetAll();
            if (allEvents.Any())
            {
                return Ok(allEvents);
            }
            else return NoContent();
        }

        /// <summary>
        /// GET: Get Event by id.
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Event event</returns>
        [HttpGet("{id}")]
        public ActionResult<Event> GetEventById(int id)
        {
            var ev = _eventService.GetById(id);
            if (ev == null)
            {
                return BadRequest();
            }
            
            return Ok(ev);
        }

        /// <summary>
        /// POST: Create new event.
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpPost]
        public ActionResult<Event> CreateEvent(Event ev)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var createdEvent = _eventService.Add(ev);

            return Ok(createdEvent);
        }

        /// <summary>
        /// PUT: Update Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpPut("{id}")]
        public ActionResult<Event> UpdateEvent(Event ev)
        {
            if (!ModelState.IsValid || _eventService.GetById(ev.Id) == null)
                return BadRequest(ModelState);

            var updatedEvent = _eventService.Update(ev);
            return Ok(updatedEvent);
        }

        /// <summary>
        /// DELETE: Delete Event
        /// </summary>
        /// <returns>Event eventt</returns>
        [HttpDelete("{id}")]
        public ActionResult<Event> DeleteEvent(int id)
        {
            if (_eventService.GetById(id) == null)
                return BadRequest();

            var deletedEvent = _eventService.Delete(id);
            return Ok(deletedEvent);
        }
    }
}
