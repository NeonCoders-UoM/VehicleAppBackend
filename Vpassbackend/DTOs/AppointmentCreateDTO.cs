using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class AppointmentCreateDTO
    {
        public int VehicleId { get; set; }
        public List<int> ServiceIds { get; set; } = new();
        public int Station_id { get; set; }
        public int CustomerId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
    }
}
