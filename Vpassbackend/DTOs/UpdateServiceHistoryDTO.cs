namespace Vpassbackend.DTOs
{
    public class UpdateServiceHistoryDTO
    {
        public int ServiceHistoryId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public int? ServiceCenterId { get; set; }
        public int? ServicedByUserId { get; set; }
        public DateTime ServiceDate { get; set; }
        public int? Mileage { get; set; }
        
        // For unverified services
        public string? ExternalServiceCenterName { get; set; }
        
        // Base64 string for receipt document (if any)
        public string? ReceiptDocument { get; set; }
    }
}
