using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        public DateTime? AppointmentDate { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }

        [MaxLength(20)]
        public string? Type { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public Service Service { get; set; }
        public Customer Customer { get; set; }
    }
}
