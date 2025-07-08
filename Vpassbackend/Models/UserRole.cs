using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.Models
{
    public class UserRole
    {
        [Key]
        public int UserRoleId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserRoleName { get; set; }
    }
}
