using System;
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
        public DateTime ServiceDate { get; set; }

        [Required]
        [MaxLength(100)]
        public required string ServiceType { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public decimal Cost { get; set; }

        [ForeignKey("ServiceCenter")]
        public int ServiceCenterId { get; set; }

        [ForeignKey("User")]
        public int? ServicedByUserId { get; set; }

        // Navigation properties
        public required Vehicle Vehicle { get; set; }
        public required ServiceCenter ServiceCenter { get; set; }
        public User? ServicedByUser { get; set; }
    }
}
