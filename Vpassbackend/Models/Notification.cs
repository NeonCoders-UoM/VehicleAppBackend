using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string Message { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Type { get; set; } // e.g., "service_reminder", "appointment", "general"

        [Required]
        [MaxLength(20)]
        public required string Priority { get; set; } // "Low", "Medium", "High", "Critical"

        [MaxLength(7)]
        public string PriorityColor { get; set; } = "#3B82F6"; // Default blue color

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        public DateTime? SentAt { get; set; }

        public DateTime? ScheduledFor { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Optional foreign keys for related entities
        public int? ServiceReminderId { get; set; }
        public int? VehicleId { get; set; }
        public int? AppointmentId { get; set; }

        // Additional context fields
        [MaxLength(20)]
        public string? VehicleRegistrationNumber { get; set; }

        [MaxLength(100)]
        public string? VehicleBrand { get; set; }

        [MaxLength(100)]
        public string? VehicleModel { get; set; }

        [MaxLength(200)]
        public string? ServiceName { get; set; }

        [MaxLength(150)]
        public string? CustomerName { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ServiceReminderId")]
        public virtual ServiceReminder? ServiceReminder { get; set; }

        [ForeignKey("VehicleId")]
        public virtual Vehicle? Vehicle { get; set; }

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }
    }
}
