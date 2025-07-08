using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vpassbackend.Models
{
    public class BorderPoint
    {
        [Key]
        public int PointId { get; set; }

        [ForeignKey("Vehicle")]
        public int VehicleId { get; set; }

        [MaxLength(100)]
        public string? CheckPoint { get; set; }

        public DateTime? CheckDate { get; set; }

        [MaxLength(20)]
        public string? EntryPoint { get; set; }

        // Navigation properties
        public Vehicle Vehicle { get; set; }
    }
}
