namespace Vpassbackend.DTOs
{
    public class ServiceReminderDTO
    {
        public int ServiceReminderId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceId { get; set; }
        public DateTime ReminderDate { get; set; }
        public int IntervalMonths { get; set; }
        public int NotifyBeforeDays { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Additional properties for convenient display
        public string ServiceName { get; set; } = string.Empty;
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public string? VehicleBrand { get; set; }
        public string? VehicleModel { get; set; }
    }
}
