using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class VehicleTransfer
    {
        [Key]
        public int TransferId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [ForeignKey("FromOwner")]
        public int FromOwnerId { get; set; }

        [ForeignKey("ToOwner")]
        public int ToOwnerId { get; set; }

        [Required]
        public DateTime InitiatedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected, Expired

        public int? MileageAtTransfer { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? SalePrice { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime ExpiresAt { get; set; }

        // Navigation properties
        public required Vehicle Vehicle { get; set; }
        public required Customer FromOwner { get; set; }
        public required Customer ToOwner { get; set; }
    }
}
