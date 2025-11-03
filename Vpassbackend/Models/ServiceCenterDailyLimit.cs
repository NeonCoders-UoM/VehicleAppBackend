using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceCenterDailyLimit
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ServiceCenter")]
        public int Station_id { get; set; }

        public DateOnly Date { get; set; }

        public int MaxAppointments { get; set; }

        public int CurrentAppointments { get; set; }

        // Navigation property
        public ServiceCenter ServiceCenter { get; set; } = null!;
    }
} 