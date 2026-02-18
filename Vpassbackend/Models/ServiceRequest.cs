using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public enum ServiceRequestStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class ServiceRequest
    {
        [Key]
        public int ServiceRequestId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ServiceName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ApprovedBasePrice { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        // Who requested it
        [Required]
        public int RequestedByUserId { get; set; }
        
        [ForeignKey(nameof(RequestedByUserId))]
        public User? RequestedBy { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // If approved, link to created service
        public int? ApprovedServiceId { get; set; }
        
        [ForeignKey(nameof(ApprovedServiceId))]
        public Service? ApprovedService { get; set; }
    }
}
