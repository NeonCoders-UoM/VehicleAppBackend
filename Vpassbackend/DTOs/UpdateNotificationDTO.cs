using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class UpdateNotificationDTO
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }

        [MaxLength(20)]
        public string? Priority { get; set; }

        [MaxLength(7)]
        public string? PriorityColor { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? ScheduledFor { get; set; }
    }
}
