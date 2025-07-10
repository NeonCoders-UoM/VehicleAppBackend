using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class ServiceCenterCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public string OwnerName { get; set; }

        [MaxLength(15)]
        public string VATNumber { get; set; }

        [MaxLength(100)]
        public string RegisterationNumber { get; set; }

        [Required]
        [MaxLength(150)]
        public string StationName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(20)]
        public string Telephone { get; set; }

        [Required]
        [MaxLength(255)]
        public string Address { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
