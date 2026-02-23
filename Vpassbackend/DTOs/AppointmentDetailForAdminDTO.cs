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
        public int VehicleId { get; set; }
        public int ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; }
        public string? Status { get; set; }
        public int CustomerLoyaltyPoints { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public decimal? AppointmentPrice { get; set; }
    }
}
