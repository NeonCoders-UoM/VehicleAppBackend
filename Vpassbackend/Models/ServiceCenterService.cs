using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceCenterService
    {
        [Key]
        public int ServiceCenterServiceId { get; set; }
        
        [ForeignKey("ServiceCenter")]
        public int Station_id { get; set; }
        
        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal? CustomPrice { get; set; }
        
        public bool IsAvailable { get; set; } = true;
        
        [MaxLength(255)]
        public string? Notes { get; set; }
        
        // Navigation properties
        public required ServiceCenter ServiceCenter { get; set; }
        public required Service Service { get; set; }
    }
}
