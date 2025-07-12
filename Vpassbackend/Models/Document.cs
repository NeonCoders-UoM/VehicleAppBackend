using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        public DocumentType DocumentType { get; set; }

        public string? DisplayName { get; set; } // for warranty documents

        public long FileSize { get; set; } // in bytes

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpirationDate { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public int? VehicleId { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }
    }
}
