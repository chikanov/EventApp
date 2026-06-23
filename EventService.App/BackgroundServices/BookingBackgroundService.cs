using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;
using static EventApp.Models.Booking;

namespace EventApp.BackgroundServices
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IEventService _eventService;
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        public BookingBackgroundService(ILogger<BookingBackgroundService> logger,
            IEventService eventService, IBookingService bookingService)
        {
            _logger = logger;
            _eventService = eventService;
            _bookingService = bookingService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
                    _logger.LogInformation("Start booking processing.");

                    var pendingBookings = _bookingService.GetPending().ToList();
                    var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, stoppingToken));

                    await Task.WhenAll(tasks);
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

        private async Task ProcessBookingAsync(Booking booking, CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            try
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                                "{DateTime} Start booking with Id: {Id} processing with status: {Status}", DateTime.Now,
                                booking.Id, booking.Status);

                    if (booking.Status == Booking.BookingStatus.Pending.ToString())
                    {
                        await _processingSemaphore.WaitAsync(stoppingToken);
                        var currentEvent = _eventService.GetById(booking.EventId);
                        if (currentEvent != null && currentEvent.TryReserveSeats())
                        {
                            booking.Confirm();
                            booking.ProcessedAt = DateTime.Now;
                            _bookingService.Update(booking);
                        } else throw new NoAvailableSeatsException();
                    }
                }
            }
            catch (NoAvailableSeatsException ex)
            {
                _logger.LogWarning(ex?.InnerException?.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when executing the ProcessBookingAsync: {ex.Message}");
                if (booking.Status == BookingStatus.Confirmed.ToString())
                {
                    await _processingSemaphore.WaitAsync(stoppingToken);
                    var currentEvent = _eventService.GetById(booking.EventId);
                    if (currentEvent != null)
                    {
                        currentEvent.ReleaseSeats();
                        booking.Reject();
                        _bookingService.Update(booking);
                        _eventService.Update(booking.EventId);
                    } else throw new NotFoundException($"Event with Id = {booking.EventId} does not exist.");
                }
            }
            finally 
            { 
                _logger.LogInformation(
                        "{DateTime} Booking with Id: {Id} has received a new status: {Status}.",DateTime.Now, booking.Id, booking.Status);
                _processingSemaphore.Release();
            }

        }
    }
}
