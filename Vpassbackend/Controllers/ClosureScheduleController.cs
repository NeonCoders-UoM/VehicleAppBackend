using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Vpassbackend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClosureScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ClosureScheduleController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> AddClosure([FromBody] ClosureSchedule closure)
        {
            bool exists = _context.ClosureSchedules.Any(c =>
                c.ServiceCenterId == closure.ServiceCenterId &&
                c.WeekNumber == closure.WeekNumber &&
                c.Day == closure.Day);
            if (exists) return BadRequest("Duplicate closure entry.");

            // Reset the ID to 0 to let Entity Framework generate it
            closure.Id = 0;

            _context.ClosureSchedules.Add(closure);
            await _context.SaveChangesAsync();

            // Update service availability for this service center on the specified day
            await UpdateServiceAvailabilityFromClosure(closure.ServiceCenterId, closure.WeekNumber, closure.Day);

            return Ok(closure);
        }

        [HttpGet("{serviceCenterId}")]
        public IActionResult GetClosures(int serviceCenterId, [FromQuery] int weekNumber)
        {
            var closures = _context.ClosureSchedules
                .Where(c => c.ServiceCenterId == serviceCenterId && c.WeekNumber == weekNumber)
                .ToList();
            return Ok(closures);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClosure(int id, [FromBody] UpdateClosureScheduleDTO updateDto)
        {
            var existingClosure = await _context.ClosureSchedules.FindAsync(id);
            if (existingClosure == null)
            {
                return NotFound("Closure schedule not found.");
            }

            // Check if another closure with the same details already exists (excluding current one)
            bool duplicateExists = _context.ClosureSchedules.Any(c =>
                c.Id != id &&
                c.ServiceCenterId == updateDto.ServiceCenterId &&
                c.WeekNumber == updateDto.WeekNumber &&
                c.Day == updateDto.Day);

            if (duplicateExists)
            {
                return BadRequest("Duplicate closure entry.");
            }

            // Store old values for service availability update
            var oldServiceCenterId = existingClosure.ServiceCenterId;
            var oldWeekNumber = existingClosure.WeekNumber;
            var oldDay = existingClosure.Day;

            existingClosure.ServiceCenterId = updateDto.ServiceCenterId;
            existingClosure.WeekNumber = updateDto.WeekNumber;
            existingClosure.Day = updateDto.Day;

            await _context.SaveChangesAsync();

            // Update service availability for both old and new closure details
            await UpdateServiceAvailabilityFromClosure(oldServiceCenterId, oldWeekNumber, oldDay);
            await UpdateServiceAvailabilityFromClosure(updateDto.ServiceCenterId, updateDto.WeekNumber, updateDto.Day);

            return Ok(existingClosure);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClosure(int id)
        {
            var closure = await _context.ClosureSchedules.FindAsync(id);
            if (closure == null)
            {
                return NotFound("Closure schedule not found.");
            }

            // Store values for service availability update
            var serviceCenterId = closure.ServiceCenterId;
            var weekNumber = closure.WeekNumber;
            var day = closure.Day;

            _context.ClosureSchedules.Remove(closure);
            await _context.SaveChangesAsync();

            // Update service availability after closure removal
            await UpdateServiceAvailabilityFromClosure(serviceCenterId, weekNumber, day);

            return Ok(new { message = "Closure schedule deleted successfully." });
        }

        private async Task UpdateServiceAvailabilityFromClosure(int serviceCenterId, int weekNumber, string? day)
        {
            // Get all services for the service center
            var serviceCenterServices = await _context.ServiceCenterServices
                .Where(scs => scs.Station_id == serviceCenterId)
                .ToListAsync();

            if (!serviceCenterServices.Any())
            {
                return; // No services to update
            }

            // Check if the service center is closed on the specified week and day
            bool isClosed = await _context.ClosureSchedules
                .AnyAsync(cs => cs.ServiceCenterId == serviceCenterId && 
                               cs.WeekNumber == weekNumber && 
                               cs.Day == day);

            // Update availability based on closure status
            foreach (var service in serviceCenterServices)
            {
                // If service center is closed, make all services unavailable
                // If service center is open, restore availability to default (true)
                service.IsAvailable = !isClosed;
            }

            await _context.SaveChangesAsync();
        }
    }
}