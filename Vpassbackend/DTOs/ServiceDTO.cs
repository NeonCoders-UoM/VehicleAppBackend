using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class ServiceDTO
    {
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public decimal? BasePrice { get; set; }

        public int? Station_id { get; set; }

        public string? StationName { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }
    }

    public class CreateServiceDTO
    {
        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public decimal? BasePrice { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }
    }

    public class UpdateServiceDTO
    {
        [MaxLength(100)]
        public string? ServiceName { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public decimal? BasePrice { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }
    }
}
