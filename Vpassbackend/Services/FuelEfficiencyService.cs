using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Vpassbackend.Services
{
    public interface IFuelEfficiencyService
    {
        Task<List<FuelEfficiencyDTO>> GetFuelRecordsByVehicleAsync(int vehicleId);
        Task<FuelEfficiencySummaryDTO> GetFuelSummaryAsync(int vehicleId, int? year = null);
        Task<FuelEfficiencyDTO> AddFuelRecordAsync(AddFuelEfficiencyDTO addFuelDto);
        Task<bool> UpdateFuelRecordAsync(int fuelRecordId, AddFuelEfficiencyDTO updateFuelDto);
        Task<bool> DeleteFuelRecordAsync(int fuelRecordId);
        Task<List<MonthlyFuelSummaryDTO>> GetMonthlyChartDataAsync(int vehicleId, int year);
        Task<List<FuelEfficiencyDTO>> GetFuelRecordsByMonthAsync(int vehicleId, int year, int month);
        Task<decimal> GetTotalFuelForPeriodAsync(int vehicleId, DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageFuelPerMonthAsync(int vehicleId, int year);
    }

    public class FuelEfficiencyService : IFuelEfficiencyService
    {
        private readonly ApplicationDbContext _context;

        public FuelEfficiencyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FuelEfficiencyDTO>> GetFuelRecordsByVehicleAsync(int vehicleId)
        {
            return await _context.FuelEfficiencies
                .Where(fe => fe.VehicleId == vehicleId)
                .OrderByDescending(fe => fe.Date)
                .Select(fe => new FuelEfficiencyDTO
                {
                    FuelEfficiencyId = fe.FuelEfficiencyId,
                    VehicleId = fe.VehicleId,
                    FuelAmount = fe.FuelAmount,
                    Date = fe.Date,
                    CreatedAt = fe.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<FuelEfficiencySummaryDTO> GetFuelSummaryAsync(int vehicleId, int? year = null)
        {
            var currentYear = year ?? DateTime.Now.Year;

            // Get vehicle information
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId);

            if (vehicle == null)
            {
                throw new ArgumentException("Vehicle not found");
            }

            // Get monthly chart data
            var monthlySummary = await GetMonthlyChartDataAsync(vehicleId, currentYear);

            var totalFuelThisYear = monthlySummary.Sum(m => m.TotalFuelAmount);
            var averageMonthlyFuel = await GetAverageFuelPerMonthAsync(vehicleId, currentYear);

            return new FuelEfficiencySummaryDTO
            {
                VehicleId = vehicleId,
                VehicleRegistrationNumber = vehicle.RegistrationNumber,
                MonthlySummary = monthlySummary,
                TotalFuelThisYear = totalFuelThisYear,
                AverageMonthlyFuel = averageMonthlyFuel
            };
        }

        public async Task<FuelEfficiencyDTO> AddFuelRecordAsync(AddFuelEfficiencyDTO addFuelDto)
        {
            // Validate vehicle exists
            var vehicleExists = await _context.Vehicles
                .AnyAsync(v => v.VehicleId == addFuelDto.VehicleId);

            if (!vehicleExists)
            {
                throw new ArgumentException("Vehicle not found");
            }

            // Validate date is not in the future
            if (addFuelDto.Date > DateTime.Now.Date)
            {
                throw new ArgumentException("Date cannot be in the future");
            }

            // Validate fuel amount
            if (addFuelDto.FuelAmount <= 0)
            {
                throw new ArgumentException("Fuel amount must be greater than zero");
            }

            var fuelEfficiency = new FuelEfficiency
            {
                VehicleId = addFuelDto.VehicleId,
                FuelAmount = addFuelDto.FuelAmount,
                Date = addFuelDto.Date,
                CreatedAt = DateTime.UtcNow
            };

            _context.FuelEfficiencies.Add(fuelEfficiency);
            await _context.SaveChangesAsync();

            return new FuelEfficiencyDTO
            {
                FuelEfficiencyId = fuelEfficiency.FuelEfficiencyId,
                VehicleId = fuelEfficiency.VehicleId,
                FuelAmount = fuelEfficiency.FuelAmount,
                Date = fuelEfficiency.Date,
                CreatedAt = fuelEfficiency.CreatedAt
            };
        }

        public async Task<bool> UpdateFuelRecordAsync(int fuelRecordId, AddFuelEfficiencyDTO updateFuelDto)
        {
            var fuelEfficiency = await _context.FuelEfficiencies
                .FirstOrDefaultAsync(fe => fe.FuelEfficiencyId == fuelRecordId);

            if (fuelEfficiency == null)
            {
                return false;
            }

            // Validate vehicle exists
            var vehicleExists = await _context.Vehicles
                .AnyAsync(v => v.VehicleId == updateFuelDto.VehicleId);

            if (!vehicleExists)
            {
                throw new ArgumentException("Vehicle not found");
            }

            // Validate date is not in the future
            if (updateFuelDto.Date > DateTime.Now.Date)
            {
                throw new ArgumentException("Date cannot be in the future");
            }

            // Validate fuel amount
            if (updateFuelDto.FuelAmount <= 0)
            {
                throw new ArgumentException("Fuel amount must be greater than zero");
            }

            fuelEfficiency.VehicleId = updateFuelDto.VehicleId;
            fuelEfficiency.FuelAmount = updateFuelDto.FuelAmount;
            fuelEfficiency.Date = updateFuelDto.Date;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFuelRecordAsync(int fuelRecordId)
        {
            var fuelEfficiency = await _context.FuelEfficiencies
                .FirstOrDefaultAsync(fe => fe.FuelEfficiencyId == fuelRecordId);

            if (fuelEfficiency == null)
            {
                return false;
            }

            _context.FuelEfficiencies.Remove(fuelEfficiency);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MonthlyFuelSummaryDTO>> GetMonthlyChartDataAsync(int vehicleId, int year)
        {
            // Get fuel efficiency records for the specified year
            var fuelRecords = await _context.FuelEfficiencies
                .Where(fe => fe.VehicleId == vehicleId && fe.Date.Year == year)
                .ToListAsync();

            // Group by month and calculate totals (accumulative within each month)
            var monthlySummary = fuelRecords
                .GroupBy(fe => new { fe.Date.Year, fe.Date.Month })
                .Select(g => new MonthlyFuelSummaryDTO
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key.Month),
                    TotalFuelAmount = g.Sum(fe => fe.FuelAmount), // Sum all fuel added in this month
                    RecordCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            // Fill in missing months with zero values for complete chart data
            var allMonths = new List<MonthlyFuelSummaryDTO>();
            for (int month = 1; month <= 12; month++)
            {
                var existingMonth = monthlySummary.FirstOrDefault(m => m.Month == month);
                if (existingMonth != null)
                {
                    allMonths.Add(existingMonth);
                }
                else
                {
                    allMonths.Add(new MonthlyFuelSummaryDTO
                    {
                        Year = year,
                        Month = month,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                        TotalFuelAmount = 0,
                        RecordCount = 0
                    });
                }
            }

            return allMonths;
        }

        public async Task<List<FuelEfficiencyDTO>> GetFuelRecordsByMonthAsync(int vehicleId, int year, int month)
        {
            return await _context.FuelEfficiencies
                .Where(fe => fe.VehicleId == vehicleId
                    && fe.Date.Year == year
                    && fe.Date.Month == month)
                .OrderBy(fe => fe.Date)
                .Select(fe => new FuelEfficiencyDTO
                {
                    FuelEfficiencyId = fe.FuelEfficiencyId,
                    VehicleId = fe.VehicleId,
                    FuelAmount = fe.FuelAmount,
                    Date = fe.Date,
                    CreatedAt = fe.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<decimal> GetTotalFuelForPeriodAsync(int vehicleId, DateTime startDate, DateTime endDate)
        {
            return await _context.FuelEfficiencies
                .Where(fe => fe.VehicleId == vehicleId
                    && fe.Date >= startDate
                    && fe.Date <= endDate)
                .SumAsync(fe => fe.FuelAmount);
        }

        public async Task<decimal> GetAverageFuelPerMonthAsync(int vehicleId, int year)
        {
            var monthlyTotals = await _context.FuelEfficiencies
                .Where(fe => fe.VehicleId == vehicleId && fe.Date.Year == year)
                .GroupBy(fe => fe.Date.Month)
                .Select(g => g.Sum(fe => fe.FuelAmount))
                .ToListAsync();

            if (!monthlyTotals.Any())
                return 0;

            return monthlyTotals.Average();
        }
    }
}
