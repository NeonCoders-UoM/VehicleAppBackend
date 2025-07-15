using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Vpassbackend.DTOs;

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

            existingClosure.ServiceCenterId = updateDto.ServiceCenterId;
            existingClosure.WeekNumber = updateDto.WeekNumber;
            existingClosure.Day = updateDto.Day;

            await _context.SaveChangesAsync();
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

            _context.ClosureSchedules.Remove(closure);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Closure schedule deleted successfully." });
        }
    }
}