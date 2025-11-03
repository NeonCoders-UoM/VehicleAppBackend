namespace Vpassbackend.DTOs
{
    public class ServiceCenterSlotsDTO
    {
        public int Id { get; set; }
        public int Station_id { get; set; }
        public DateOnly Date { get; set; }
        public int UsedSlots { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
    }
}