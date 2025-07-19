using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.DTOs
{
    public class ServiceCenterServiceDTO
    {
        public int ServiceCenterServiceId { get; set; }
        public int Station_id { get; set; }
        public int ServiceId { get; set; }
        public int? PackageId { get; set; }
        public decimal? CustomPrice { get; set; }
        public decimal? ServiceCenterBasePrice { get; set; }
        public int? ServiceCenterLoyaltyPoints { get; set; }
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }

        // Include related data
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal? ServiceBasePrice { get; set; }
        public string? Category { get; set; }
        public string? StationName { get; set; }
        
        // Package information
        public string? PackageName { get; set; }
        public decimal? PackagePercentage { get; set; }
        public string? PackageDescription { get; set; }
    }

    public class CreateServiceCenterServiceDTO
    {
        [Required]
        public int Station_id { get; set; }

        [Required]
        public int ServiceId { get; set; }

        public int? PackageId { get; set; }

        public decimal? CustomPrice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ServiceCenterBasePrice { get; set; }

        public bool IsAvailable { get; set; } = true;

        [MaxLength(255)]
        public string? Notes { get; set; }
    }

    public class UpdateServiceCenterServiceDTO
    {
        public int? PackageId { get; set; }

        public decimal? CustomPrice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ServiceCenterBasePrice { get; set; }

        public bool? IsAvailable { get; set; }

        [MaxLength(255)]
        public string? Notes { get; set; }
    }
}
