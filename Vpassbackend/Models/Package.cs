using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Package
    {
        [Key]
        public int PackageId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string PackageName { get; set; }

        [Required]
        [Range(0, 100)]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal Percentage { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<ServiceCenterService> ServiceCenterServices { get; set; } = new List<ServiceCenterService>();
    }
} 