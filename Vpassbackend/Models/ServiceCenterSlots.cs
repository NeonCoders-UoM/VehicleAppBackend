using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceCenterSlots
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ServiceCenter")]
        public int Station_id { get; set; }

        public DateOnly Date { get; set; }

        public int UsedSlots { get; set; }

        // Navigation property
        public ServiceCenter ServiceCenter { get; set; } = null!;
    }
}
