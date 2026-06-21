using EventApp.Models;

namespace EventApp.Interfaces
{
    public interface IBookingQueue
    {
        void Enqueue(Booking task);
        bool TryDequeue(out Booking task);
    }
}
