using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class CreateServiceReminderDTO
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime ReminderDate { get; set; }

        [Required]
        [Range(1, 60, ErrorMessage = "Interval months must be between 1 and 60")]
        public int IntervalMonths { get; set; }

        [Required]
        [Range(1, 90, ErrorMessage = "Notification days must be between 1 and 90")]
        public int NotifyBeforeDays { get; set; }

        public string? Notes { get; set; }
    }
}
