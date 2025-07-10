using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ServiceName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? BasePrice { get; set; }

        public int? LoyaltyPoints { get; set; }

        // Service category or type could be added here
        [MaxLength(50)]
        public string? Category { get; set; }

        // Navigation properties
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Many-to-many relationship with ServiceCenters through ServiceCenterService
        public ICollection<ServiceCenterService> ServiceCenterServices { get; set; } = new List<ServiceCenterService>();
    }
}
