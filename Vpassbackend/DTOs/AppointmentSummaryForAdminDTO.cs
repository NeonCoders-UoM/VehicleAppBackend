namespace Vpassbackend.DTOs
{
    public class AppointmentSummaryForAdminDTO
    {
        public int AppointmentId { get; set; }
        public string OwnerName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Status { get; set; }
    }
}
