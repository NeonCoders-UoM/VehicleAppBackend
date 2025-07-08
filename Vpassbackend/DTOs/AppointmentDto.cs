namespace Vpassbackend.DTOs
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceId { get; set; }
        public int CustomerId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }

        public VehicleDto? Vehicle { get; set; }
        public ServiceDto? Service { get; set; }
        public CustomerDto? Customer { get; set; }
    }

    public class AppointmentCreateDto
    {
        public int VehicleId { get; set; }
        public int ServiceId { get; set; }
        public int CustomerId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
    }

    public class AppointmentUpdateDto
    {
        public DateTime? AppointmentDate { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }
    }
}
