using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;
using System.Globalization;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuelEfficiencyController : ControllerBase
    {
        private readonly IFuelEfficiencyService _fuelEfficiencyService;

        public FuelEfficiencyController(IFuelEfficiencyService fuelEfficiencyService)
        {
            _fuelEfficiencyService = fuelEfficiencyService;
        }

        // GET: api/FuelEfficiency/vehicle/{vehicleId}
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<List<FuelEfficiencyDTO>>> GetFuelEfficienciesByVehicle(int vehicleId)
        {
            try
            {
                var fuelEfficiencies = await _fuelEfficiencyService.GetFuelRecordsByVehicleAsync(vehicleId);
                return Ok(fuelEfficiencies);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/FuelEfficiency/vehicle/{vehicleId}/summary
        [HttpGet("vehicle/{vehicleId}/summary")]
        public async Task<ActionResult<FuelEfficiencySummaryDTO>> GetFuelEfficiencySummary(int vehicleId, [FromQuery] int? year = null)
        {
            try
            {
                var summary = await _fuelEfficiencyService.GetFuelSummaryAsync(vehicleId, year);
                return Ok(summary);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/FuelEfficiency/vehicle/{vehicleId}/chart/{year}
        [HttpGet("vehicle/{vehicleId}/chart/{year}")]
        public async Task<ActionResult<List<MonthlyFuelSummaryDTO>>> GetMonthlyChartData(int vehicleId, int year)
        {
            try
            {
                var chartData = await _fuelEfficiencyService.GetMonthlyChartDataAsync(vehicleId, year);
                return Ok(chartData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/FuelEfficiency
        [HttpPost]
        public async Task<ActionResult<FuelEfficiencyDTO>> AddFuelEfficiency(AddFuelEfficiencyDTO addFuelDto)
        {
            try
            {
                var result = await _fuelEfficiencyService.AddFuelRecordAsync(addFuelDto);
                return CreatedAtAction(nameof(GetFuelEfficienciesByVehicle), 
                    new { vehicleId = result.VehicleId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while adding fuel record");
            }
        }

        // PUT: api/FuelEfficiency/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFuelEfficiency(int id, AddFuelEfficiencyDTO updateFuelDto)
        {
            try
            {
                var success = await _fuelEfficiencyService.UpdateFuelRecordAsync(id, updateFuelDto);
                if (!success)
                {
                    return NotFound("Fuel efficiency record not found");
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while updating fuel record");
            }
        }

        // DELETE: api/FuelEfficiency/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFuelEfficiency(int id)
        {
            try
            {
                var success = await _fuelEfficiencyService.DeleteFuelRecordAsync(id);
                if (!success)
                {
                    return NotFound("Fuel efficiency record not found");
                }
                return NoContent();
            }
            catch (Exception)
            {
                return BadRequest("An error occurred while deleting fuel record");
            }
        }

        // GET: api/FuelEfficiency/vehicle/{vehicleId}/monthly/{year}/{month}
        [HttpGet("vehicle/{vehicleId}/monthly/{year}/{month}")]
        public async Task<ActionResult<List<FuelEfficiencyDTO>>> GetMonthlyFuelRecords(int vehicleId, int year, int month)
        {
            try
            {
                var fuelRecords = await _fuelEfficiencyService.GetFuelRecordsByMonthAsync(vehicleId, year, month);
                return Ok(fuelRecords);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/FuelEfficiency/vehicle/{vehicleId}/period
        [HttpGet("vehicle/{vehicleId}/period")]
        public async Task<ActionResult<decimal>> GetFuelForPeriod(int vehicleId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var totalFuel = await _fuelEfficiencyService.GetTotalFuelForPeriodAsync(vehicleId, startDate, endDate);
                return Ok(new { TotalFuel = totalFuel, StartDate = startDate, EndDate = endDate });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/FuelEfficiency/vehicle/{vehicleId}/average/{year}
        [HttpGet("vehicle/{vehicleId}/average/{year}")]
        public async Task<ActionResult<decimal>> GetAverageFuelPerMonth(int vehicleId, int year)
        {
            try
            {
                var averageFuel = await _fuelEfficiencyService.GetAverageFuelPerMonthAsync(vehicleId, year);
                return Ok(new { AverageMonthlyFuel = averageFuel, Year = year });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
