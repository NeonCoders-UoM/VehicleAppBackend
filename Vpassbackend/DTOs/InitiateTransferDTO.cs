using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class InitiateTransferDTO
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        [EmailAddress]
        public required string BuyerEmail { get; set; }

        public int? MileageAtTransfer { get; set; }

        public decimal? SalePrice { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
