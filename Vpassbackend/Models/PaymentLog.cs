using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class PaymentLog
    {
        [Key]
        public int LogId { get; set; }

        [ForeignKey("Invoice")]
        public int? InvoiceId { get; set; }

        [ForeignKey("Appointment")]
        public int? AppointmentId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        [MaxLength(50)]
        public string? PaymentType { get; set; } // Advance, Full, Partial

        // Navigation properties
        public Invoice? Invoice { get; set; }
        public Appointment? Appointment { get; set; }
    }
}
