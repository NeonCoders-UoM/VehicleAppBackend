using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class DocumentUploadRequest
    {
        [Required]
        public IFormFile File { get; set; } = null!;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int DocumentType { get; set; }

        public int? VehicleId { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public string? DisplayName { get; set; }
    }
}
