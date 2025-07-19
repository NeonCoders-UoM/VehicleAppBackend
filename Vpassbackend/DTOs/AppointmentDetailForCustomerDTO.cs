namespace Vpassbackend.DTOs
{
    public class AppointmentDetailForCustomerDTO
    {
        public string VehicleRegistration { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public int ServiceCenterId { get; set; }
        public string? ServiceCenterAddress { get; set; }
        public string? ServiceCenterName { get; set; }
        public double? DistanceInKm { get; set; } // GPS logic
        public double LoyaltyPoints { get; set; }

        public List<AppointmentServiceDetailDTO> Services { get; set; } = new();
        public decimal TotalCost { get; set; }
    }
}
