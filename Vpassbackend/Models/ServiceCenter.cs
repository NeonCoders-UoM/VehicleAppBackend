using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class ServiceCenter
    {
        [Key]
        public int Station_id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string OwnerName { get; set; }

        [MaxLength(15)]
        public string? VATNumber { get; set; }

        [MaxLength(100)]
        public string? RegisterationNumber { get; set; }

        [MaxLength(150)]
        public string? Station_name { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Telephone { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? Station_status { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int DefaultDailyAppointmentLimit { get; set; } = 20; // Default appointment limit per day

        // Navigation properties
        public ICollection<ServiceCenterCheckInPoint> CheckInPoints { get; set; } = new List<ServiceCenterCheckInPoint>();

        // Many-to-many relationship with Services through ServiceCenterService
        public ICollection<ServiceCenterService> ServiceCenterServices { get; set; } = new List<ServiceCenterService>();
    }
}
