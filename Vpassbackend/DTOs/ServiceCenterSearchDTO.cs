namespace Vpassbackend.DTOs
{
    public class ServiceCenterSearchDTO
    {
        public string? SearchTerm { get; set; } // General keyword search (name, email, etc.)
        public string? Status { get; set; }     // "active", "inactive"
        public int PageNumber { get; set; } = 1; // For pagination
        public int PageSize { get; set; } = 10;  // For pagination
    }
}
