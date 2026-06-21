using EventApp.Interfaces;
using System.Collections.Concurrent;

namespace EventApp.Models
{
    public class InMemoryBookingQueue : IBookingQueue
    {
        private readonly ConcurrentQueue<Booking> _queue = new();
        public void Enqueue(Booking booking)
        {
            _queue.Enqueue(booking);
        }

        public bool TryDequeue(out Booking booking)
        {
            return _queue.TryDequeue(out booking);
        }
    }
}
