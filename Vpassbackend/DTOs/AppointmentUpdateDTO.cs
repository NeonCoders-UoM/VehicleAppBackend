namespace Vpassbackend.DTOs
{
    public class AppointmentUpdateDTO
    {
        public DateTime? AppointmentDate { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
        public List<int>? ServiceIds { get; set; }
    }
}
