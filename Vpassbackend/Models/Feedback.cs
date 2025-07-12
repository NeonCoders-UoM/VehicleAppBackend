using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("ServiceCenter")]
        public int ServiceCenterId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comments { get; set; }

        public DateTime FeedbackDate { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? ServiceDate { get; set; }

        // Navigation properties
        public required Customer Customer { get; set; }
        public required ServiceCenter ServiceCenter { get; set; }
        public required Vehicle Vehicle { get; set; }
    }
}
