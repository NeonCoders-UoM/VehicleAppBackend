using System.ComponentModel.DataAnnotations;

namespace Vpassbackend.DTOs
{
    public class AddFuelEfficiencyDTO
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        [Range(0.01, 1000, ErrorMessage = "Fuel amount must be between 0.01 and 1000 liters")]
        public decimal FuelAmount { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }

    public class FuelEfficiencyDTO
    {
        public int FuelEfficiencyId { get; set; }
        public int VehicleId { get; set; }
        public decimal FuelAmount { get; set; }
        public DateTime Date { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MonthlyFuelSummaryDTO
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalFuelAmount { get; set; }
        public int RecordCount { get; set; }
    }

    public class FuelEfficiencySummaryDTO
    {
        public int VehicleId { get; set; }
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public List<MonthlyFuelSummaryDTO> MonthlySummary { get; set; } = new List<MonthlyFuelSummaryDTO>();
        public decimal TotalFuelThisYear { get; set; }
        public decimal AverageMonthlyFuel { get; set; }
    }
}
