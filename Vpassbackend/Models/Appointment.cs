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

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("ServiceCenter")]
        public int ServiceCenterId { get; set; }

        public DateTime AppointmentDate { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Completed, Cancelled

        [MaxLength(255)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal EstimatedTotalCost { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal AdvancePaymentAmount { get; set; } // 10% of the estimated cost

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ActualTotalCost { get; set; } // Final cost after service completion

        public bool IsAdvancePaymentCompleted { get; set; } = false;

        // Navigation properties
        public Vehicle Vehicle { get; set; }
        public Customer Customer { get; set; }
        public ServiceCenter ServiceCenter { get; set; }
        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();
        public ICollection<PaymentLog> PaymentLogs { get; set; } = new List<PaymentLog>();
    }

    public class AppointmentService
    {
        [Key]
        public int AppointmentServiceId { get; set; }

        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ServicePrice { get; set; }

        // Navigation properties
        public Appointment Appointment { get; set; }
        public Service Service { get; set; }
    }
}
