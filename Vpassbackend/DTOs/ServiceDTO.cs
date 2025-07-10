using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class ServiceDTO
    {
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public decimal? BasePrice { get; set; }

        public int StationId { get; set; }
    }
}
