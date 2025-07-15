namespace Vpassbackend.Models
{
    public class InvoiceDetailsDTO
    {
        public string RegistrationNumber { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public decimal TotalCost { get; set; }
        public int LoyaltyPoints { get; set; }
        public int ServiceCenterId { get; set; }
        public string Address { get; set; }
        public decimal DistanceKm { get; set; } = 25; // Default distance
        public string PaymentStatus { get; set; }
        public List<ServiceCostDTO> Services { get; set; } = new();
    }
}
