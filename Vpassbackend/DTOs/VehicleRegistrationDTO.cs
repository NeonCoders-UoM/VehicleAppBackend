using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class VehicleRegistrationDTO
    {
        [Required]
        [MaxLength(50)]
        public string RegistrationNumber { get; set; }

        public int CustomerId { get; set; }

        [MaxLength(100)]
        public string Brand { get; set; }

        [MaxLength(100)]
        public string Model { get; set; }

        [MaxLength(20)]
        public string ChassisNumber { get; set; }

        public int? Mileage { get; set; }

        [MaxLength(50)]
        public string Fuel { get; set; }

        public int? Year { get; set; }
    }
}
