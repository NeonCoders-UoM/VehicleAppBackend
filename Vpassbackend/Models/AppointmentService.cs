using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class AppointmentService
    {
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }

        public decimal? ServicePrice { get; set; }

        // Navigation properties
        public Appointment Appointment { get; set; }
        public Service Service { get; set; }
    }
}
