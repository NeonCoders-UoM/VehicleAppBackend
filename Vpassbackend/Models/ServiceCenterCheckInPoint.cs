using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceCenterCheckInPoint
    {
        [Key]
        public int StationId { get; set; }
        
        [ForeignKey("ServiceCenter")]
        public int ServiceId { get; set; }
        
        [MaxLength(50)]
        public string? Name { get; set; }
        
        // Navigation properties
        public ServiceCenter ServiceCenter { get; set; }
    }
}
