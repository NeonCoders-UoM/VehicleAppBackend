using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationsController> _logger;
        private readonly INotificationService _notificationService;

        public NotificationsController(ApplicationDbContext context, ILogger<NotificationsController> logger, INotificationService notificationService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
        }

        // GET: api/Notifications/Customer/5
        [HttpGet("Customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetCustomerNotifications(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.CustomerId == customerId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificationDTO
                    {
                        NotificationId = n.NotificationId,
                        CustomerId = n.CustomerId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        Priority = n.Priority,
                        PriorityColor = n.PriorityColor,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        SentAt = n.SentAt,
                        ScheduledFor = n.ScheduledFor,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt,
                        ServiceReminderId = n.ServiceReminderId,
                        VehicleId = n.VehicleId,
                        AppointmentId = n.AppointmentId,
                        VehicleRegistrationNumber = n.VehicleRegistrationNumber,
                        VehicleBrand = n.VehicleBrand,
                        VehicleModel = n.VehicleModel,
                        ServiceName = n.ServiceName,
                        CustomerName = n.CustomerName
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {notifications.Count} notifications for customer {customerId}");
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notifications for customer {customerId}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Notifications/Customer/5/Unread
        [HttpGet("Customer/{customerId}/Unread")]
        public async Task<ActionResult<IEnumerable<NotificationDTO>>> GetUnreadNotifications(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.CustomerId == customerId && !n.IsRead)
                    .OrderByDescending(n => n.CreatedAt)
                    .Select(n => new NotificationDTO
                    {
                        NotificationId = n.NotificationId,
                        CustomerId = n.CustomerId,
                        Title = n.Title,
                        Message = n.Message,
                        Type = n.Type,
                        Priority = n.Priority,
                        PriorityColor = n.PriorityColor,
                        IsRead = n.IsRead,
                        ReadAt = n.ReadAt,
                        SentAt = n.SentAt,
                        ScheduledFor = n.ScheduledFor,
                        CreatedAt = n.CreatedAt,
                        UpdatedAt = n.UpdatedAt,
                        ServiceReminderId = n.ServiceReminderId,
                        VehicleId = n.VehicleId,
                        AppointmentId = n.AppointmentId,
                        VehicleRegistrationNumber = n.VehicleRegistrationNumber,
                        VehicleBrand = n.VehicleBrand,
                        VehicleModel = n.VehicleModel,
                        ServiceName = n.ServiceName,
                        CustomerName = n.CustomerName
                    })
                    .ToListAsync();

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving unread notifications for customer {customerId}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationDTO>> GetNotification(int id)
        {
            try
            {
                var notification = await _context.Notifications
                    .FirstOrDefaultAsync(n => n.NotificationId == id);

                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                var notificationDto = new NotificationDTO
                {
                    NotificationId = notification.NotificationId,
                    CustomerId = notification.CustomerId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Priority = notification.Priority,
                    PriorityColor = notification.PriorityColor,
                    IsRead = notification.IsRead,
                    ReadAt = notification.ReadAt,
                    SentAt = notification.SentAt,
                    ScheduledFor = notification.ScheduledFor,
                    CreatedAt = notification.CreatedAt,
                    UpdatedAt = notification.UpdatedAt,
                    ServiceReminderId = notification.ServiceReminderId,
                    VehicleId = notification.VehicleId,
                    AppointmentId = notification.AppointmentId,
                    VehicleRegistrationNumber = notification.VehicleRegistrationNumber,
                    VehicleBrand = notification.VehicleBrand,
                    VehicleModel = notification.VehicleModel,
                    ServiceName = notification.ServiceName,
                    CustomerName = notification.CustomerName
                };

                return Ok(notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notification {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notifications
        [HttpPost]
        public async Task<ActionResult<NotificationDTO>> CreateNotification(CreateNotificationDTO createDto)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(createDto.CustomerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                // Set priority color based on priority if not provided
                var priorityColor = createDto.PriorityColor;
                if (string.IsNullOrEmpty(priorityColor))
                {
                    priorityColor = (createDto.Priority ?? "medium").ToLower() switch
                    {
                        "critical" => "#DC2626", // Red
                        "high" => "#EA580C",     // Orange
                        "medium" => "#3B82F6",   // Blue
                        "low" => "#10B981",      // Green
                        _ => "#3B82F6"             // Default blue
                    };
                }

                var notification = new Notification
                {
                    CustomerId = createDto.CustomerId,
                    Title = createDto.Title,
                    Message = createDto.Message,
                    Type = createDto.Type,
                    Priority = createDto.Priority,
                    PriorityColor = priorityColor,
                    ScheduledFor = createDto.ScheduledFor,
                    ServiceReminderId = createDto.ServiceReminderId,
                    VehicleId = createDto.VehicleId,
                    AppointmentId = createDto.AppointmentId,
                    VehicleRegistrationNumber = createDto.VehicleRegistrationNumber,
                    VehicleBrand = createDto.VehicleBrand,
                    VehicleModel = createDto.VehicleModel,
                    ServiceName = createDto.ServiceName,
                    CustomerName = createDto.CustomerName,
                    CreatedAt = DateTime.UtcNow,
                    SentAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                var notificationDto = new NotificationDTO
                {
                    NotificationId = notification.NotificationId,
                    CustomerId = notification.CustomerId,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    Priority = notification.Priority,
                    PriorityColor = notification.PriorityColor,
                    IsRead = notification.IsRead,
                    ReadAt = notification.ReadAt,
                    SentAt = notification.SentAt,
                    ScheduledFor = notification.ScheduledFor,
                    CreatedAt = notification.CreatedAt,
                    UpdatedAt = notification.UpdatedAt,
                    ServiceReminderId = notification.ServiceReminderId,
                    VehicleId = notification.VehicleId,
                    AppointmentId = notification.AppointmentId,
                    VehicleRegistrationNumber = notification.VehicleRegistrationNumber,
                    VehicleBrand = notification.VehicleBrand,
                    VehicleModel = notification.VehicleModel,
                    ServiceName = notification.ServiceName,
                    CustomerName = notification.CustomerName
                };

                return CreatedAtAction(nameof(GetNotification), new { id = notification.NotificationId }, notificationDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Notifications/5/MarkAsRead
        [HttpPut("{id}/MarkAsRead")]
        public async Task<ActionResult> MarkNotificationAsRead(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                if (!notification.IsRead)
                {
                    notification.IsRead = true;
                    notification.ReadAt = DateTime.UtcNow;
                    notification.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Marked notification {id} as read");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {id} as read");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Notifications/Customer/5/MarkAllAsRead
        [HttpPut("Customer/{customerId}/MarkAllAsRead")]
        public async Task<ActionResult> MarkAllNotificationsAsRead(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                var unreadNotifications = await _context.Notifications
                    .Where(n => n.CustomerId == customerId && !n.IsRead)
                    .ToListAsync();

                if (unreadNotifications.Any())
                {
                    var currentTime = DateTime.UtcNow;
                    foreach (var notification in unreadNotifications)
                    {
                        notification.IsRead = true;
                        notification.ReadAt = currentTime;
                        notification.UpdatedAt = currentTime;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Marked {unreadNotifications.Count} notifications as read for customer {customerId}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking all notifications as read for customer {customerId}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Notifications/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateNotification(int id, UpdateNotificationDTO updateDto)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                // Update only provided fields
                if (!string.IsNullOrEmpty(updateDto.Title))
                    notification.Title = updateDto.Title;

                if (!string.IsNullOrEmpty(updateDto.Message))
                    notification.Message = updateDto.Message;

                if (!string.IsNullOrEmpty(updateDto.Priority))
                    notification.Priority = updateDto.Priority;

                if (!string.IsNullOrEmpty(updateDto.PriorityColor))
                    notification.PriorityColor = updateDto.PriorityColor;

                if (updateDto.IsRead.HasValue)
                {
                    notification.IsRead = updateDto.IsRead.Value;
                    if (updateDto.IsRead.Value && notification.ReadAt == null)
                    {
                        notification.ReadAt = DateTime.UtcNow;
                    }
                }

                if (updateDto.ScheduledFor.HasValue)
                    notification.ScheduledFor = updateDto.ScheduledFor;

                notification.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation($"Updated notification {id}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating notification {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Notifications/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotification(int id)
        {
            try
            {
                var notification = await _context.Notifications.FindAsync(id);
                if (notification == null)
                {
                    return NotFound("Notification not found");
                }

                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted notification {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting notification {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Notifications/Customer/5/DeleteAll
        [HttpDelete("Customer/{customerId}/DeleteAll")]
        public async Task<ActionResult> DeleteAllCustomerNotifications(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                var notifications = await _context.Notifications
                    .Where(n => n.CustomerId == customerId)
                    .ToListAsync();

                if (notifications.Any())
                {
                    _context.Notifications.RemoveRange(notifications);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Deleted {notifications.Count} notifications for customer {customerId}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting all notifications for customer {customerId}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Notifications/Customer/5/Count
        [HttpGet("Customer/{customerId}/Count")]
        public async Task<ActionResult<object>> GetNotificationCount(int customerId)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound("Customer not found");
                }

                var totalCount = await _context.Notifications
                    .CountAsync(n => n.CustomerId == customerId);

                var unreadCount = await _context.Notifications
                    .CountAsync(n => n.CustomerId == customerId && !n.IsRead);

                return Ok(new
                {
                    TotalCount = totalCount,
                    UnreadCount = unreadCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting notification count for customer {customerId}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Notifications/GenerateFromServiceReminders
        [HttpPost("GenerateFromServiceReminders")]
        public async Task<ActionResult> GenerateNotificationsFromServiceReminders()
        {
            try
            {
                var today = DateTime.UtcNow.Date;

                // Get all active service reminders that should trigger notifications
                var remindersToNotify = await _context.ServiceReminders
                    .Include(sr => sr.Vehicle)
                    .Include(sr => sr.Service)
                    .Where(sr => sr.IsActive &&
                           sr.ReminderDate.Date <= today.AddDays(sr.NotifyBeforeDays))
                    .ToListAsync();

                int notificationsCreated = 0;

                foreach (var reminder in remindersToNotify)
                {
                    // Check if we've already created a notification for this reminder recently
                    var existingNotification = await _context.Notifications
                        .FirstOrDefaultAsync(n => n.ServiceReminderId == reminder.ServiceReminderId &&
                                                 n.CreatedAt.Date >= today.AddDays(-1)); // Within last day

                    if (existingNotification == null)
                    {
                        // Get customer info
                        var vehicle = await _context.Vehicles
                            .Include(v => v.Customer)
                            .FirstOrDefaultAsync(v => v.VehicleId == reminder.VehicleId);

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

                            _context.Notifications.Add(notification);
                            notificationsCreated++;
                        }
                    }
                }

                if (notificationsCreated > 0)
                {
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Generated {notificationsCreated} notifications from service reminders");

                return Ok(new { NotificationsCreated = notificationsCreated });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating notifications from service reminders");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
