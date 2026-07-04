using EventApp.CustomExceptions;
using EventApp.DataAccess;
using EventApp.Interfaces;
using EventApp.Models;
using EventApp.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace EventApp.Services
{
    /// <summary>
    /// EventService
    /// </summary>
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;
        public EventService(AppDbContext context)
        {
            _context = context;
        }
        ///GetAll() 
        public async Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Events.AsNoTrackingWithIdentityResolution().Include(e => e.Bookings).ToListAsync(cancellationToken);
        }
        ///GetAll() 
        public async Task<PaginatedResult> GetAllAsync(int page, int pageSize, string? Title = null, 
            DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        {
            return await GetEventListAsync(Title, from, to, page, pageSize, cancellationToken);
        }

        ///GetById
        public async Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
                ?? throw new NotFoundException("Event not found");
            return @event;
        }

        /// Add
        public async Task<Event> CreateEventAsync(CreateEventDto ev, CancellationToken cancellationToken = default)
        {
            if (ev.TotalSeats <= 0)
            {
                throw new ValidationException(nameof(ev.TotalSeats), "Total seats value must be greater than zero.");
            }
            var newEventId = await _context.Events.AnyAsync(cancellationToken) ? await _context.Events.MaxAsync(e => e.Id, cancellationToken) + 1 : 1;
            var newEvent = Event.Create(newEventId, ev.Title, ev.Description, ev.StartAt, ev.EndAt, ev.TotalSeats);

            await _context.Events.AddAsync(newEvent, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return newEvent;
        }

        /// Update
        public async Task<Event> UpdateEventAsync(int id, EventDto ev, CancellationToken cancellationToken = default)
        {
            var existEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (existEvent == null)
            {
                throw new NotFoundException($"Event with Id = {id} does not exist.");
            }

            if (ev.StartAt > ev.EndAt)
            {
                throw new ValidationException(nameof(ev.StartAt), "The end date must be greater than the start date.");
            }

            if (ev.TotalSeats <= 0)
            {
                throw new ValidationException(nameof(ev.TotalSeats), "Total seats value must be greater than zero.");
            }

            if (existEvent != null)
            {
                existEvent.Update(ev.Title, ev.Description, ev.StartAt, ev.EndAt, ev.TotalSeats, ev.AvailableSeats);
            }
            await _context.SaveChangesAsync(cancellationToken);

            return existEvent;
        }

        public async Task<Event> UpdateEventAsync(int id, Event ev, CancellationToken cancellationToken = default)
        {
            var existEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (existEvent == null)
            {
                throw new NotFoundException($"Event with Id = {id} does not exist.");
            }

            if (existEvent.StartAt > existEvent.EndAt)
            {
                throw new ValidationException(nameof(existEvent.StartAt), "The end date must be greater than the start date.");
            }

            if (existEvent.TotalSeats <= 0)
            {
                throw new ValidationException(nameof(existEvent.TotalSeats), "Total seats value must be greater than zero.");
            }

            if (existEvent!= null)
            {
                existEvent.Update(ev.Title, ev.Description, ev.StartAt, ev.EndAt, ev.TotalSeats, ev.AvailableSeats);
            }
            await _context.SaveChangesAsync(cancellationToken);
            return existEvent;
        }

        /// Delete
        public async Task<bool> DeleteEventAsync(int id, CancellationToken cancellationToken = default)
        {
            var existEvent = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
            if (existEvent == null)
            {
                throw new NotFoundException($"Event with Id = {id} does not exist.");
            }
            if (existEvent != null)
                _context.Events.Remove(existEvent);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        public async Task<PaginatedResult> GetEventListAsync(
            string? title,
            DateTime? from,
            DateTime? to,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var events = _context.Events.AsNoTrackingWithIdentityResolution().Include(e => e.Bookings).AsQueryable();

            if (!string.IsNullOrEmpty(title))
            {
                events = events.Where(e => e.Title.Contains(title ?? "", StringComparison.OrdinalIgnoreCase));
            }

            if (from != null)
            {
                events = events.Where(e => e.StartAt >= GetTheStartOfTheDayOrDefault(from));
            }

            if (to != null)
            {
                events = events.Where(e => e.EndAt <= GetTheEndOfTheDayOrDefault(to));
            }

            int filteredCount = events.Count();

            var items = await events.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

            int totalPages = (int)Math.Ceiling((double)filteredCount / pageSize);

            return new PaginatedResult() { 
                CountEventsOnPage = items.Count,
                ListEvents = items, 
                EventsCount = filteredCount, 
                Page = page
            };
        }

        DateTime? GetTheStartOfTheDayOrDefault(DateTime? from)
        {
            return from!.Value.AddHours(0 - from.Value.Hour).
                AddMinutes(0 - from.Value.Minute).
                AddSeconds(0 - from.Value.Second);
        }

        DateTime? GetTheEndOfTheDayOrDefault(DateTime? to)
        {
            return to!.Value.AddHours(23 - to.Value.Hour).
                AddMinutes(59 - to.Value.Minute).
                AddSeconds(59 - to.Value.Second);
        }
    }
}
