namespace Vpassbackend.Models
{
    public class ServiceAvailability
    {
        public int Id { get; set; }
        public int ServiceCenterId { get; set; }
        public int ServiceId { get; set; }
        public DateTime Date { get; set; } // Specific date for availability
        public bool IsAvailable { get; set; }
    }
}