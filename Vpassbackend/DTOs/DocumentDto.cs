namespace Vpassbackend.DTOs
{
    public class DocumentDto
    {
        public int DocumentId { get; set; }
        public int VehicleId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? FilePath { get; set; }
    }

    public class DocumentCreateDto
    {
        public int VehicleId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? FilePath { get; set; }
    }

    public class DocumentUpdateDto
    {
        public string? DocumentType { get; set; }
        public string? DocumentPath { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? FilePath { get; set; }
    }
}
