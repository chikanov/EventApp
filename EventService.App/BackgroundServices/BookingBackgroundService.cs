using EventApp.CustomExceptions;
using EventApp.Interfaces;
using EventApp.Models;
using static EventApp.Models.Booking;

namespace EventApp.BackgroundServices
{
    public class BookingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventService _eventService;
        private readonly ILogger<BookingBackgroundService> _logger;
        private readonly SemaphoreSlim _processingSemaphore = new(1, 1);
        private const int PollingInterval = 2;
        private const int ProcessingDelay = 10;

        public BookingBackgroundService(IServiceProvider serviceProvider, ILogger<BookingBackgroundService> logger,
            IEventService eventService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _eventService = eventService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingBackgroundService is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(ProcessingDelay), stoppingToken);
                    _logger.LogInformation("Start booking processing.");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var _bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                        var pendingBookings = _bookingService.GetPending().ToList();
                        var tasks = pendingBookings.Select(booking => ProcessBookingAsync(booking, _bookingService, stoppingToken));

                        await Task.WhenAll(tasks);
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
                await Task.Delay(TimeSpan.FromSeconds(PollingInterval), stoppingToken);
            }
            _logger.LogInformation("BookingBackgroundService has been stopped.");
        }

        private async Task ProcessBookingAsync(Booking booking, IBookingService _bookingService, CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(PollingInterval), stoppingToken);

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
                        if (currentEvent != null)
                        {
                            booking.Confirm();
                            _bookingService.Update(booking);
                        } else throw new NotFoundException($"Event with Id = {booking.EventId} does not exist.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error when executing the ProcessBookingAsync: {ex.Message}");
                if (booking.Status == BookingStatus.Confirmed.ToString() || booking.Status == BookingStatus.Pending.ToString())
                {
                    await _processingSemaphore.WaitAsync(stoppingToken);
                    var currentEvent = _eventService.GetById(booking.EventId);
                    if (currentEvent != null)
                    {
                        currentEvent.ReleaseSeats();
                        booking.Reject();
                        _bookingService.Update(booking);
                        _eventService.Update(booking.EventId, currentEvent);
                    } else throw new NotFoundException($"Event with Id = {booking.EventId} does not exist.");
                }
            }
            finally
            {
                _logger.LogInformation(
                        "{DateTime} Booking with Id: {Id} has received a new status: {Status}.", DateTime.Now, booking.Id, booking.Status);
                if (booking.Status == BookingStatus.Confirmed.ToString())
                {
                    _processingSemaphore.Release();
                }
            }
        }
    }
}
