using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string FirstName { get; set; }

        [MaxLength(100)]
        public string LastName { get; set; }

        [Required, EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [ForeignKey("UserRole")]
        public int UserRoleId { get; set; }
        public virtual UserRole UserRole { get; set; }
<<<<<<< Updated upstream
=======

        // Add foreign key to ServiceCenter for ServiceCenterAdmin users
        [ForeignKey("ServiceCenter")]
        public int? Station_id { get; set; }
        public virtual ServiceCenter? ServiceCenter { get; set; }

        // Forgot password fields
        public string? ForgotPasswordOtp { get; set; }
        public DateTime? ForgotPasswordOtpExpiry { get; set; }
>>>>>>> Stashed changes
    }
}
