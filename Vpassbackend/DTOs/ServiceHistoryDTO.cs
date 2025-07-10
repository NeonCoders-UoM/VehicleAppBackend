namespace Vpassbackend.DTOs
{
    public class ServiceHistoryDTO
    {
        public int ServiceHistoryId { get; set; }
        public int VehicleId { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Cost { get; set; }
        public int? ServiceCenterId { get; set; }
        public int? ServicedByUserId { get; set; }
        public string? ServiceCenterName { get; set; }
        public string? ServicedByUserName { get; set; }
        public DateTime ServiceDate { get; set; }
        public int? Mileage { get; set; }
        public bool IsVerified { get; set; }
        public string? ExternalServiceCenterName { get; set; }
        public string? ReceiptDocumentPath { get; set; }
    }
}
