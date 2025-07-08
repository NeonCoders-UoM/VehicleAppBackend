using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? TotalCost { get; set; }

        public DateTime? InvoiceDate { get; set; }

        // Navigation properties
        public required Vehicle Vehicle { get; set; }

        public ICollection<PaymentLog> PaymentLogs { get; set; }

        public Invoice()
        {
            PaymentLogs = new List<PaymentLog>();
        }
    }
}
