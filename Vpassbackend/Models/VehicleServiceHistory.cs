using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class VehicleServiceHistory
    {
        [Key]
        public int ServiceHistoryId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Cost { get; set; }

        [ForeignKey("ServiceCenter")]
        public int? ServiceCenterId { get; set; }

        [ForeignKey("User")]
        public int? ServicedByUserId { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        public int? Mileage { get; set; }

        [Required]
        public bool IsVerified { get; set; } = false;

        // For unverified services, customer can provide the service center name manually
        [MaxLength(150)]
        public string? ExternalServiceCenterName { get; set; }

        // Optional invoice/receipt document reference
        [MaxLength(255)]
        public string? ReceiptDocumentPath { get; set; }

        // Navigation properties
        public required Vehicle Vehicle { get; set; }
        public ServiceCenter? ServiceCenter { get; set; }
        public User? ServicedByUser { get; set; }
    }
}
