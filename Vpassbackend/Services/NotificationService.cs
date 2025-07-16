using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;

namespace Vpassbackend.Services
{
    public interface INotificationService
    {
        Task CleanupOrphanedNotificationsAsync();
        Task CreateServiceReminderNotificationAsync(ServiceReminder serviceReminder);
        Task CreateAppointmentNotificationAsync(Appointment appointment, string message);
        Task CreateGeneralNotificationAsync(int customerId, string title, string message, string priority = "Medium");
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CleanupOrphanedNotificationsAsync()
        {
            try
            {
                // Find notifications with invalid foreign key references
                var orphanedNotifications = await _context.Notifications
                    .Where(n =>
                        (n.CustomerId != null && !_context.Customers.Any(c => c.CustomerId == n.CustomerId)) ||
                        (n.ServiceReminderId != null && !_context.ServiceReminders.Any(sr => sr.ServiceReminderId == n.ServiceReminderId)) ||
                        (n.VehicleId != null && !_context.Vehicles.Any(v => v.VehicleId == n.VehicleId)) ||
                        (n.AppointmentId != null && !_context.Appointments.Any(a => a.AppointmentId == n.AppointmentId)))
                    .ToListAsync();

                if (orphanedNotifications.Any())
                {
                    _context.Notifications.RemoveRange(orphanedNotifications);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Cleaned up {orphanedNotifications.Count} orphaned notifications");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up orphaned notifications");
            }
        }

        public async Task CreateServiceReminderNotificationAsync(ServiceReminder serviceReminder)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Customer)
                    .FirstOrDefaultAsync(v => v.VehicleId == serviceReminder.VehicleId);

                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceReminder.ServiceId);

                if (vehicle?.Customer != null)
                {
                    var today = DateTime.UtcNow.Date;
                    var daysUntilDue = (serviceReminder.ReminderDate.Date - today).Days;
                    var priorityLevel = daysUntilDue <= 0 ? "Critical" :
                                      daysUntilDue <= 3 ? "High" : "Medium";

                    var title = daysUntilDue <= 0
                        ? $"{service?.ServiceName ?? "Service"} Overdue"
                        : $"{service?.ServiceName ?? "Service"} Due Soon";

                    var message = daysUntilDue <= 0
                        ? $"Your {vehicle.RegistrationNumber} is overdue for {service?.ServiceName ?? "service"}. Please schedule an appointment immediately."
                        : $"Your {vehicle.RegistrationNumber} needs {service?.ServiceName ?? "service"} in {daysUntilDue} day{(daysUntilDue == 1 ? "" : "s")}. Please schedule an appointment.";

                    var notification = new Notification
                    {
                        CustomerId = vehicle.CustomerId,
                        Title = title,
                        Message = message,
                        Type = "service_reminder",
                        Priority = priorityLevel,
                        PriorityColor = priorityLevel switch
                        {
                            "Critical" => "#DC2626", // Red
                            "High" => "#EA580C",     // Orange
                            "Medium" => "#3B82F6",   // Blue
                            _ => "#3B82F6"           // Default blue
                        },
                        ServiceReminderId = serviceReminder.ServiceReminderId,
                        VehicleId = serviceReminder.VehicleId,
                        VehicleRegistrationNumber = vehicle.RegistrationNumber,
                        VehicleBrand = vehicle.Brand,
                        VehicleModel = vehicle.Model,
                        ServiceName = service?.ServiceName,
                        CustomerName = $"{vehicle.Customer.FirstName} {vehicle.Customer.LastName}",
                        CreatedAt = DateTime.UtcNow,
                        SentAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Created service reminder notification for customer {vehicle.CustomerId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating service reminder notification for reminder {serviceReminder.ServiceReminderId}");
            }
        }

        public async Task CreateAppointmentNotificationAsync(Appointment appointment, string message)
        {
            try
            {
                var appointmentWithDetails = await _context.Appointments
                    .Include(a => a.Customer)
                    .Include(a => a.Vehicle)
                    .Include(a => a.Service)
                    .Include(a => a.ServiceCenter)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

                if (appointmentWithDetails?.Customer != null)
                {
                    var notification = new Notification
                    {
                        CustomerId = appointmentWithDetails.CustomerId,
                        Title = "Appointment Update",
                        Message = message,
                        Type = "appointment",
                        Priority = "Medium",
                        PriorityColor = "#3B82F6",
                        AppointmentId = appointment.AppointmentId,
                        VehicleId = appointmentWithDetails.VehicleId,
                        VehicleRegistrationNumber = appointmentWithDetails.Vehicle?.RegistrationNumber,
                        VehicleBrand = appointmentWithDetails.Vehicle?.Brand,
                        VehicleModel = appointmentWithDetails.Vehicle?.Model,
                        ServiceName = appointmentWithDetails.Service?.ServiceName,
                        CustomerName = $"{appointmentWithDetails.Customer.FirstName} {appointmentWithDetails.Customer.LastName}",
                        CreatedAt = DateTime.UtcNow,
                        SentAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Created appointment notification for customer {appointmentWithDetails.CustomerId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating appointment notification for appointment {appointment.AppointmentId}");
            }
        }

        public async Task CreateGeneralNotificationAsync(int customerId, string title, string message, string priority = "Medium")
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer != null)
                {
                    var notification = new Notification
                    {
                        CustomerId = customerId,
                        Title = title,
                        Message = message,
                        Type = "general",
                        Priority = priority,
                        PriorityColor = priority.ToLower() switch
                        {
                            "critical" => "#DC2626",
                            "high" => "#EA580C",
                            "medium" => "#3B82F6",
                            "low" => "#10B981",
                            _ => "#3B82F6"
                        },
                        CustomerName = $"{customer.FirstName} {customer.LastName}",
                        CreatedAt = DateTime.UtcNow,
                        SentAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"Created general notification for customer {customerId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating general notification for customer {customerId}");
            }
        }
    }
}
