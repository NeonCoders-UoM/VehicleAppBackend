using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(200)]
        public string Address { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }

        [MaxLength(20)]
        public string NIC { get; set; }

        public int LoyaltyPoints { get; set; }

        public bool IsEmailVerified { get; set; } = false;
        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }

        // Forgot password fields
        public string? ForgotPasswordOtp { get; set; }
        public DateTime? ForgotPasswordOtpExpiry { get; set; }

        // Push notification fields
        [MaxLength(300)]
        public string? DeviceToken { get; set; }

        public DateTime? LastTokenUpdate { get; set; }

        [MaxLength(50)]
        public string? Platform { get; set; }

        public bool PushNotificationsEnabled { get; set; } = false;

        // Google Auth fields
        [MaxLength(50)]
        public string AuthProvider { get; set; } = "local"; // "local" or "google"

        [MaxLength(200)]
        public string? GoogleId { get; set; }
    }
}
