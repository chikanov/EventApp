using EventService.Application.Abstractions.Persistence.Repositories;
using EventService.Domain.Models.Enum;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventService.Application.BackgroundServices
{
    public class BookingBackgroundService : BackgroundService
    {
        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BookingBackgroundService> _logger;

        public BookingBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<BookingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    List<Guid> pendingBookingIds;

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                        var pendingBooking = await bookingRepository.GetPendingAsync(stoppingToken);
                        pendingBookingIds = pendingBooking.Select(b => b.Id).ToList();
                    }

                    var tasks = pendingBookingIds.Select(id =>
                        ProcessBookingAsync(id, stoppingToken));

                    await Task.WhenAll(tasks); 
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing pending bookings");
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }

        private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(ProcessingDelay, stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                var booking = await bookingRepository.GetByIdAsync(bookingId, stoppingToken);
                if (booking == null || booking.Status != BookingStatus.Pending)
                    return;

                var @event = await eventRepository.GetByIdAsync(booking.EventId, stoppingToken);
                if (@event == null)
                {
                    booking.Reject();
                    await bookingRepository.SaveChangesAsync(stoppingToken);

                    _logger.LogWarning(
                        "Booking {BookingId} rejected: event {EventId} not found",
                        booking.Id, booking.EventId);

                    return;
                }

                booking.Confirm();
                await bookingRepository.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Booking {BookingId} for event {EventId} processed → {Status}",
                    booking.Id, booking.EventId, booking.Status);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

                    var booking = await bookingRepository.GetByIdAsync(bookingId, stoppingToken);
                    if (booking != null)
                    {
                        booking.Reject();

                        var @event = await eventRepository.GetByIdAsync(booking.EventId, stoppingToken);
                        if (@event != null)
                            @event.ReleaseSeats();

                        await bookingRepository.SaveChangesAsync(stoppingToken);
                        await eventRepository.SaveChangesAsync(stoppingToken);
                    }

                    _logger.LogError(ex,
                        "Booking {BookingId} rejected due to processing error",
                        bookingId);
                }
                catch (Exception releaseEx)
                {
                    _logger.LogError(releaseEx,
                        "Failed to reject booking {BookingId} after error",
                        bookingId);
                }
            }
        }
    }
}
