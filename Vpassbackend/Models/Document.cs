using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [MaxLength(100)]
        public string? DocumentType { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        [MaxLength(255)]
        public string? FilePath { get; set; }

        // Navigation properties
        public required Vehicle Vehicle { get; set; }
    }
}
