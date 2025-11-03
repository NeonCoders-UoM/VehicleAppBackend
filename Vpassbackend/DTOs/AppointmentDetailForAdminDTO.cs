namespace Vpassbackend.DTOs
{
    public class AppointmentDetailForAdminDTO
    {
        public int AppointmentId { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public string OwnerName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public List<string> Services { get; set; }
        public int VehicleId { get; set; } // <-- Add this
        public int ServiceCenterId { get; set; } // <-- Add this
        public string ServiceCenterName { get; set; } // <-- Optional, for display
    }
}
