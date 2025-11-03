namespace Vpassbackend.Models
{
    public class ClosureSchedule
    {
        public int Id { get; set; }
        public int ServiceCenterId { get; set; }
        public DateTime ClosureDate { get; set; }
    }
}