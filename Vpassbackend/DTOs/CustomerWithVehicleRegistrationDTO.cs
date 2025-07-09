using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class CustomerWithVehicleRegistrationDTO
    {
        // Customer information
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [MaxLength(20)]
        public string NIC { get; set; }

        // Vehicle information
        [Required]
        [MaxLength(50)]
        public string RegistrationNumber { get; set; }

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
