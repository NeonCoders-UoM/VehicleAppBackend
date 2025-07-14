using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class FuelEfficiency
    {
        [Key]
        public int FuelEfficiencyId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelAmount { get; set; } // Amount of fuel in liters

        [Required]
        public DateTime Date { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Vehicle? Vehicle { get; set; }
    }
}
