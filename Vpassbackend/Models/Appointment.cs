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

        [ForeignKey("ServiceCenter")]
        public int Station_id { get; set; }

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
        public ServiceCenter ServiceCenter { get; set; }
        public Customer Customer { get; set; }

        // Calculated price at the time of appointment
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? AppointmentPrice { get; set; }

        // Many-to-many relationship for selected services
        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
    }
}
