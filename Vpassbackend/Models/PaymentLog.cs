using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class PaymentLog
    {
        [Key]
        public int LogId { get; set; }
        
        [ForeignKey("Invoice")]
        public int InvoiceId { get; set; }
        
        public DateTime? PaymentDate { get; set; }
        
        [MaxLength(20)]
        public string? Status { get; set; }
        
        // Navigation properties
        public Invoice Invoice { get; set; }
    }
}
