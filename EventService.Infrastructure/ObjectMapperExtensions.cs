using EventService.Application.DTOs;
using EventService.Domain.Entities;

namespace EventService.Infrastructure
{
    public static class ObjectMapperExtensions
    {
        public static EventDto MapEventToEventDto(Event ev)
        {
            var newEventDto = new EventDto();
            newEventDto.Title = ev.Title;
            newEventDto.Description = ev.Description;
            newEventDto.StartAt = ev.StartAt;
            newEventDto.EndAt = ev.EndAt;
            newEventDto.TotalSeats = ev.TotalSeats;
            newEventDto.AvailableSeats = ev.AvailableSeats;

            return newEventDto;
        }
    }
}
