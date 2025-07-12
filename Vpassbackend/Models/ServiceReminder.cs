using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceReminder
    {
        [Key]
        public int ServiceReminderId { get; set; }

        [Required]
        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        // ServiceId is no longer required in the frontend but still needed for the database relationship
        [ForeignKey("Service")]
        public int ServiceId { get; set; } = 1; // Default to service ID 1

        [Required]
        public DateTime ReminderDate { get; set; }

        // Time interval in months before next service is due
        [Required]
        public int IntervalMonths { get; set; }

        // How many days before the due date to send notifications
        [Required]
        public int NotifyBeforeDays { get; set; }

        // Additional custom message for the reminder
        [MaxLength(255)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; } = null!;
        public Service Service { get; set; } = null!;
    }
}
