using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class PackageDTO
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = string.Empty;
        public decimal Percentage { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePackageDTO
    {
        [Required]
        [MaxLength(50)]
        public required string PackageName { get; set; }

        [Required]
        [Range(0, 100)]
        public decimal Percentage { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdatePackageDTO
    {
        [MaxLength(50)]
        public string? PackageName { get; set; }

        [Range(0, 100)]
        public decimal? Percentage { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
} 