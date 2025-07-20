namespace Vpassbackend.DTOs
{
    public class UpdateAvailabilityFromClosureDTO
    {
        public int ServiceCenterId { get; set; }
        public int WeekNumber { get; set; }
        public string? Day { get; set; } // e.g., "Monday"
    }
} 