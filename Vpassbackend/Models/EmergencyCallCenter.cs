using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.Models
{
    public class EmergencyCallCenter
    {
        [Key]
        public int CenterId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string RegistrationNumber { get; set; } = null!;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = null!;
    }
}
