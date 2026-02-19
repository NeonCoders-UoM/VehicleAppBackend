namespace Vpassbackend.DTOs
{
    public class TransferResponseDTO
    {
        public int TransferId { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? RegistrationNumber { get; set; }
        public int FromOwnerId { get; set; }
        public string? FromOwnerName { get; set; }
        public string? FromOwnerEmail { get; set; }
        public int ToOwnerId { get; set; }
        public string? ToOwnerName { get; set; }
        public string? ToOwnerEmail { get; set; }
        public DateTime InitiatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? MileageAtTransfer { get; set; }
        public decimal? SalePrice { get; set; }
        public string? Notes { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
