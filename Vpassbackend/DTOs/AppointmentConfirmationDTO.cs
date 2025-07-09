using System;

namespace Vpassbackend.DTOs
{
    public class AppointmentConfirmationDTO
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public required string ServiceCenterName { get; set; }
        public required string ServiceCenterAddress { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AdvancePayment { get; set; }
        public required string PaymentStatus { get; set; }
    }

    public class AppointmentPaymentDTO
    {
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public required string PaymentMethod { get; set; }
        public required string TransactionReference { get; set; }
    }
}
