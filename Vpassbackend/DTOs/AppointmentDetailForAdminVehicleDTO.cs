namespace Vpassbackend.DTOs
{
    public class AppointmentDetailForAdminVehicleDTO
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StationName { get; set; } = null!;
        public List<string> Services { get; set; } = new();
        public string? Notes { get; set; }
        public decimal BookingFee { get; set; }
    }
}
