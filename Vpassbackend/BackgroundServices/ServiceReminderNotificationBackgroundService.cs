using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Microsoft.EntityFrameworkCore;

namespace Vpassbackend.BackgroundServices
{
    public class ServiceReminderNotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceReminderNotificationBackgroundService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Run every hour

        public ServiceReminderNotificationBackgroundService(IServiceProvider serviceProvider, ILogger<ServiceReminderNotificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Add initial delay to allow app to fully start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        // Set command timeout for this context instance
                        context.Database.SetCommandTimeout(120);

                        var today = DateTime.UtcNow.Date;

                        var remindersToNotify = await context.ServiceReminders
                            .Include(sr => sr.Vehicle)
                            .Include(sr => sr.Service)
                            .Where(sr => sr.IsActive &&
                                   sr.ReminderDate.Date <= today.AddDays(sr.NotifyBeforeDays))
                            .ToListAsync(stoppingToken);

                        int notificationsCreated = 0;

                        foreach (var reminder in remindersToNotify)
                        {
                            var existingNotification = await context.Notifications
                                .FirstOrDefaultAsync(n => n.ServiceReminderId == reminder.ServiceReminderId &&
                                                         n.CreatedAt.Date >= today.AddDays(-1), stoppingToken);

                            if (existingNotification == null)
                            {
                                var vehicle = await context.Vehicles
                                    .Include(v => v.Customer)
                                    .FirstOrDefaultAsync(v => v.VehicleId == reminder.VehicleId, stoppingToken);

                                if (vehicle?.Customer != null)
                                {
                                    var daysUntilDue = (reminder.ReminderDate.Date - today).Days;
                                    var priorityLevel = daysUntilDue <= 0 ? "Critical" :
                                                      daysUntilDue <= 3 ? "High" : "Medium";

                                    var title = daysUntilDue <= 0
                                        ? $"{reminder.Service?.ServiceName ?? "Service"} Overdue"
                                        : $"{reminder.Service?.ServiceName ?? "Service"} Due Soon";

                                    var message = daysUntilDue <= 0
                                        ? $"Your {vehicle.RegistrationNumber} is overdue for {reminder.Service?.ServiceName ?? "service"}. Please schedule an appointment immediately."
                                        : $"Your {vehicle.RegistrationNumber} needs {reminder.Service?.ServiceName ?? "service"} in {daysUntilDue} day{(daysUntilDue == 1 ? "" : "s")}. Please schedule an appointment.";

                                    var notification = new Notification
                                    {
                                        CustomerId = vehicle.CustomerId,
                                        Title = title,
                                        Message = message,
                                        Type = "service_reminder",
                                        Priority = priorityLevel,
                                        PriorityColor = priorityLevel switch
                                        {
                                            "Critical" => "#DC2626",
                                            "High" => "#EA580C",
                                            "Medium" => "#3B82F6",
                                            _ => "#3B82F6"
                                        },
                                        ServiceReminderId = reminder.ServiceReminderId,
                                        VehicleId = reminder.VehicleId,
                                        VehicleRegistrationNumber = vehicle.RegistrationNumber,
                                        VehicleBrand = vehicle.Brand,
                                        VehicleModel = vehicle.Model,
                                        ServiceName = reminder.Service?.ServiceName,
                                        CustomerName = $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}",
                                        CreatedAt = DateTime.UtcNow,
                                        SentAt = DateTime.UtcNow
                                    };

                                    context.Notifications.Add(notification);
                                    notificationsCreated++;
                                }
                            }
                        }

                        if (notificationsCreated > 0)
                        {
                            await context.SaveChangesAsync(stoppingToken);
                        }

                        _logger.LogInformation($"[ServiceReminderNotificationBackgroundService] Generated {notificationsCreated} notifications from service reminders at {DateTime.UtcNow}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ServiceReminderNotificationBackgroundService] Error generating notifications from service reminders");
                    // Wait longer before retrying after error
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}