using System;
using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class AddServiceHistoryDTO
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ServiceType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public decimal Cost { get; set; }

        [Required]
        public int ServiceCenterId { get; set; }

        public int? ServicedByUserId { get; set; }

        public DateTime? ServiceDate { get; set; }
    }
}
