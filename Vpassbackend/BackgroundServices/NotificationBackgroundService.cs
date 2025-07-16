using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Services;

namespace Vpassbackend.BackgroundServices
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(6); // Run every 6 hours

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateServiceReminderNotifications();
                    await CleanupOrphanedNotifications();
                    await Task.Delay(_period, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing notification background service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 minutes before retrying
                }
            }
        }

        private async Task GenerateServiceReminderNotifications()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var today = DateTime.UtcNow.Date;

                // Get all active service reminders that should trigger notifications
                var remindersToNotify = await context.ServiceReminders
                    .Include(sr => sr.Vehicle)
                        .ThenInclude(v => v.Customer)
                    .Include(sr => sr.Service)
                    .Where(sr => sr.IsActive &&
                               sr.ReminderDate.Date <= today.AddDays(sr.NotifyBeforeDays))
                    .ToListAsync();

                int notificationsCreated = 0;

                foreach (var reminder in remindersToNotify)
                {
                    // Check if we've already created a notification for this reminder recently
                    var existingNotification = await context.Notifications
                        .FirstOrDefaultAsync(n => n.ServiceReminderId == reminder.ServiceReminderId &&
                                                 n.CreatedAt.Date >= today.AddDays(-1)); // Within last day

                    if (existingNotification == null)
                    {
                        await notificationService.CreateServiceReminderNotificationAsync(reminder);
                        notificationsCreated++;
                    }
                }

                if (notificationsCreated > 0)
                {
                    _logger.LogInformation($"Generated {notificationsCreated} notifications from service reminders");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating notifications from service reminders");
            }
        }

        private async Task CleanupOrphanedNotifications()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.CleanupOrphanedNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up orphaned notifications");
            }
        }
    }
}
