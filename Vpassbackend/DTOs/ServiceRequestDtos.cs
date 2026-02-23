using System.ComponentModel.DataAnnotations;
using Vpassbackend.Models;

namespace Vpassbackend.DTOs
{
    // DTO for creating a service request
    public class CreateServiceRequestDto
    {
        [Required]
        [MaxLength(100)]
        public required string ServiceName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }
    }

    // DTO for approving a request
    public class ApproveServiceRequestDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base price must be greater than 0")]
        public decimal BasePrice { get; set; }
    }

    // DTO for rejecting a request
    public class RejectServiceRequestDto
    {
        [Required]
        [MaxLength(500)]
        public required string RejectionReason { get; set; }
    }

    // DTO for returning service request data
    public class ServiceRequestDto
    {
        public int ServiceRequestId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? ApprovedBasePrice { get; set; }
        public string? RejectionReason { get; set; }
        
        public int RequestedByUserId { get; set; }
        public string? RequestedByName { get; set; }
        public string? RequestedByEmail { get; set; }
        public DateTime RequestedAt { get; set; }
        
        public int? ServiceCenterId { get; set; }
        public string? ServiceCenterName { get; set; }
        
        public int? ApprovedServiceId { get; set; }
    }
}
