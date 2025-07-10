using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class ServiceCenterServiceDTO
    {
        public int ServiceCenterServiceId { get; set; }
        public int Station_id { get; set; }
        public int ServiceId { get; set; }
        public decimal? CustomPrice { get; set; }
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }
        
        // Include related data
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal? BasePrice { get; set; }
        public int? LoyaltyPoints { get; set; }
        public string? Category { get; set; }
        public string? StationName { get; set; }
    }

    public class CreateServiceCenterServiceDTO
    {
        [Required]
        public int Station_id { get; set; }
        
        [Required]
        public int ServiceId { get; set; }
        
        public decimal? CustomPrice { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [MaxLength(255)]
        public string? Notes { get; set; }
    }

    public class UpdateServiceCenterServiceDTO
    {
        public decimal? CustomPrice { get; set; }
        
        public bool? IsAvailable { get; set; }
        
        [MaxLength(255)]
        public string? Notes { get; set; }
    }
}
