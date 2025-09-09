namespace Vpassbackend.DTOs
{
    public class UpdateServiceAvailabilityDTO
    {
        public int ServiceCenterId { get; set; }
        public int ServiceId { get; set; }
        public DateTime Date { get; set; } // Specific date for availability
        public bool IsAvailable { get; set; }
    }
}
