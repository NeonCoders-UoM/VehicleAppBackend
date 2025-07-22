namespace Vpassbackend.DTOs
{
    public class InvoiceDetailsDTO
    {
        public string RegistrationNumber { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public decimal TotalCost { get; set; }
        public int LoyaltyPoints { get; set; }
        public int ServiceCenterId { get; set; }
        public string Address { get; set; }
        public double? DistanceInKm { get; set; } // GPS logic
        public string PaymentStatus { get; set; }
        public List<AppointmentServiceDetailDTO> Services { get; set; } = new();
    }
}
