namespace Vpassbackend.Models
{
    public class ClosureSchedule
    {
        public int Id { get; set; }
        public int ServiceCenterId { get; set; }
        public int WeekNumber { get; set; }
        public string? Day { get; set; } // e.g., "Monday"
    }
}