namespace Vpassbackend.DTOs
{
    public class UpdateServiceAvailabilityDTO
    {
        public int ServiceCenterId { get; set; }
        public int ServiceId { get; set; }
        public int WeekNumber { get; set; }
        public string? Day { get; set; } // e.g., "Monday"
        public bool IsAvailable { get; set; }
    }
}
