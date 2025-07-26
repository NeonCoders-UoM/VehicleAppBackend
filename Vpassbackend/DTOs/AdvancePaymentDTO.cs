namespace Vpassbackend.DTOs
{
    public class AdvancePaymentCalculationDTO
    {
        public int AppointmentId { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AdvancePaymentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentType { get; set; } = "Advance"; // "Advance" or "Full"
        public string ServiceCenterName { get; set; } = string.Empty;
        public string VehicleRegistration { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public List<AppointmentServiceDetailDTO> Services { get; set; } = new();
    }

    public class AdvancePaymentRequestDTO
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty; // "card", "mobile_money", etc.
    }

    public class AppointmentPaymentDTO
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AdvancePaymentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? PaymentDate { get; set; }
        public string OrderId { get; set; } = string.Empty;
    }
} 