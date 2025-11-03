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
            // Validate that ClosureDate is not the default DateTime value
            if (closure.ClosureDate == default(DateTime))
            {
                return BadRequest("Closure date must be provided and cannot be the default date.");
            }

            // Validate that ClosureDate is not in the past
            if (closure.ClosureDate.Date < DateTime.Now.Date)
            {
                return BadRequest("Closure date cannot be in the past.");
            }

            bool exists = _context.ClosureSchedules.Any(c =>
                c.ServiceCenterId == closure.ServiceCenterId &&
                c.ClosureDate.Date == closure.ClosureDate.Date);
            if (exists) return BadRequest("Duplicate closure entry.");

            // Reset the ID to 0 to let Entity Framework generate it
            closure.Id = 0;

            _context.ClosureSchedules.Add(closure);
            await _context.SaveChangesAsync();

            // Update service availability for this service center on the specified date
            await UpdateServiceAvailabilityFromClosure(closure.ServiceCenterId, closure.ClosureDate);

            return Ok(closure);
        }

        [HttpGet("{serviceCenterId}")]
        public IActionResult GetClosures(int serviceCenterId, [FromQuery] DateTime date)
        {
            var closures = _context.ClosureSchedules
                .Where(c => c.ServiceCenterId == serviceCenterId && c.ClosureDate.Date == date.Date)
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

            // Validate that ClosureDate is not the default DateTime value
            if (updateDto.ClosureDate == default(DateTime))
            {
                return BadRequest("Closure date must be provided and cannot be the default date.");
            }

            // Validate that ClosureDate is not in the past
            if (updateDto.ClosureDate.Date < DateTime.Now.Date)
            {
                return BadRequest("Closure date cannot be in the past.");
            }

            // Check if another closure with the same details already exists (excluding current one)
            bool duplicateExists = _context.ClosureSchedules.Any(c =>
                c.Id != id &&
                c.ServiceCenterId == updateDto.ServiceCenterId &&
                c.ClosureDate.Date == updateDto.ClosureDate.Date);

            if (duplicateExists)
            {
                return BadRequest("Duplicate closure entry.");
            }

            // Store old values for service availability update
            var oldServiceCenterId = existingClosure.ServiceCenterId;
            var oldClosureDate = existingClosure.ClosureDate;

            existingClosure.ServiceCenterId = updateDto.ServiceCenterId;
            existingClosure.ClosureDate = updateDto.ClosureDate;

            await _context.SaveChangesAsync();

            // Update service availability for both old and new closure details
            await UpdateServiceAvailabilityFromClosure(oldServiceCenterId, oldClosureDate);
            await UpdateServiceAvailabilityFromClosure(updateDto.ServiceCenterId, updateDto.ClosureDate);

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
            var closureDate = closure.ClosureDate;

            _context.ClosureSchedules.Remove(closure);
            await _context.SaveChangesAsync();

            // Update service availability after closure removal
            await UpdateServiceAvailabilityFromClosure(serviceCenterId, closureDate);

            return Ok(new { message = "Closure schedule deleted successfully." });
        }

        private async Task UpdateServiceAvailabilityFromClosure(int serviceCenterId, DateTime closureDate)
        {
            // Get all services for the service center
            var serviceCenterServices = await _context.ServiceCenterServices
                .Where(scs => scs.Station_id == serviceCenterId)
                .ToListAsync();

            if (!serviceCenterServices.Any())
            {
                return; // No services to update
            }

            // Check if the service center is closed on the specified date
            bool isClosed = await _context.ClosureSchedules
                .AnyAsync(cs => cs.ServiceCenterId == serviceCenterId &&
                               cs.ClosureDate.Date == closureDate.Date);

            // Update or create service availability records for this date
            foreach (var service in serviceCenterServices)
            {
                var existingAvailability = await _context.ServiceAvailabilities
                    .FirstOrDefaultAsync(sa => sa.ServiceCenterId == serviceCenterId &&
                                             sa.ServiceId == service.ServiceId &&
                                             sa.Date.Date == closureDate.Date);

                if (existingAvailability != null)
                {
                    // Update existing record
                    existingAvailability.IsAvailable = !isClosed;
                }
                else
                {
                    // Create new availability record
                    var newAvailability = new ServiceAvailability
                    {
                        ServiceCenterId = serviceCenterId,
                        ServiceId = service.ServiceId,
                        Date = closureDate.Date,
                        IsAvailable = !isClosed
                    };
                    _context.ServiceAvailabilities.Add(newAvailability);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}