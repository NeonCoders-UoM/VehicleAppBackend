using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class ServiceCenterUpdateDTO
    {
        [MaxLength(100)]
        public string OwnerName { get; set; }

        [MaxLength(15)]
        public string VATNumber { get; set; }

        [MaxLength(100)]
        public string RegisterationNumber { get; set; }

        [MaxLength(150)]
        public string StationName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Telephone { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        [MaxLength(20)]
        public string StationStatus { get; set; }
    }
}
