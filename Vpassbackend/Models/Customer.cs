using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(255)]
        public string? Password { get; set; }

        [MaxLength(20)]
        public string? NIC { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(255)]
        public string? ProfilePicture { get; set; }

        public DateTime? LoyaltyPoints { get; set; }

        public bool? ProfileActive { get; set; }

        // Navigation properties
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
