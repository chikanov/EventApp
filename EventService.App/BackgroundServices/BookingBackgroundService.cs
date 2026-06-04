using EventApp.Interfaces;
using EventApp.Models;

namespace EventApp.BackgroundServices
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IBookingQueue _bookingQueue;
        private readonly ILogger<InMemoryBookingQueue> _logger;
        public BookingBackgroundService(IBookingQueue bookingQueue, ILogger<InMemoryBookingQueue> logger)
        {
            _bookingQueue = bookingQueue;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_bookingQueue.TryDequeue(out var booking))
                    {
                        _logger.LogInformation(
                            "Start booking with Id: {Id} processing with status: {Status}",
                            booking.Id, booking.Status);

                        if (booking.Status == Booking.BookingStatus.Pending.ToString())
                        {
                            booking.Status = Booking.BookingStatus.Confirmed.ToString();
                            booking.ProcessedAt = DateTime.Now;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                        _logger.LogInformation(
                            "Booking with Id: {Id} has received a new status: {Status}.", booking.Id, booking.Status);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error when executing the BookingBackgroundService: {ex.Message}");
                }
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            _logger.LogInformation("BookingBackgroundService has been stopped.");
        }
    }
}
