using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public int CustomerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityColor { get; set; } = "#3B82F6";
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime? ScheduledFor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ServiceReminderId { get; set; }
        public int? VehicleId { get; set; }
        public int? AppointmentId { get; set; }
        public string? VehicleRegistrationNumber { get; set; }
        public string? VehicleBrand { get; set; }
        public string? VehicleModel { get; set; }
        public string? ServiceName { get; set; }
        public string? CustomerName { get; set; }
    }
}
