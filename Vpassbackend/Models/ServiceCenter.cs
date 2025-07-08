using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceCenter
    {
        [Key]
        public int ServiceId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string OwnerName { get; set; }
        
        [MaxLength(15)]
        public string? VATNumber { get; set; }
        
        [MaxLength(100)]
        public string? RegisterationNumber { get; set; }
        
        [MaxLength(150)]
        public string? BusinessName { get; set; }
        
        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
        
        [MaxLength(20)]
        public string? Telephone { get; set; }
        
        [MaxLength(255)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? AccountStatus { get; set; }
        
        // Navigation properties
        public ICollection<Service> Services { get; set; } = new List<Service>();
        public ICollection<ServiceCenterCheckInPoint> CheckInPoints { get; set; } = new List<ServiceCenterCheckInPoint>();
    }
}
