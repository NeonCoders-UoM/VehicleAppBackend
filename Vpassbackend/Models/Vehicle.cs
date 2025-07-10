using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [MaxLength(50)]
        public required string RegistrationNumber { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(100)]
        public string? Model { get; set; }

        [MaxLength(20)]
        public string? ChassisNumber { get; set; }

        public int? Mileage { get; set; }

        [MaxLength(50)]
        public string? Fuel { get; set; }

        public int? Year { get; set; }

        // Navigation properties
        public required Customer Customer { get; set; }
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<BorderPoint> BorderPoints { get; set; } = new List<BorderPoint>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
        public ICollection<VehicleServiceHistory> ServiceHistory { get; set; } = new List<VehicleServiceHistory>();
        public ICollection<ServiceReminder> ServiceReminders { get; set; } = new List<ServiceReminder>();
    }
}
