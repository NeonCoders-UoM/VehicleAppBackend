using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class CreateNotificationDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        [MaxLength(7)]
        public string PriorityColor { get; set; } = "#3B82F6";

        public DateTime? ScheduledFor { get; set; }

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
    }
}
