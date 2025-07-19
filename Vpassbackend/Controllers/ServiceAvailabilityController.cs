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
    public class ServiceAvailabilityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServiceAvailabilityController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> AddServiceAvailability([FromBody] ServiceAvailabilityDTO serviceAvailabilityDto)
        {
            bool exists = _context.ServiceAvailabilities.Any(sa =>
                sa.ServiceCenterId == serviceAvailabilityDto.ServiceCenterId &&
                sa.ServiceId == serviceAvailabilityDto.ServiceId &&
                sa.WeekNumber == serviceAvailabilityDto.WeekNumber &&
                sa.Day == serviceAvailabilityDto.Day);
            
            if (exists) 
                return BadRequest("Duplicate service availability entry.");

            var serviceAvailability = new ServiceAvailability
            {
                ServiceCenterId = serviceAvailabilityDto.ServiceCenterId,
                ServiceId = serviceAvailabilityDto.ServiceId,
                WeekNumber = serviceAvailabilityDto.WeekNumber,
                Day = serviceAvailabilityDto.Day,
                IsAvailable = serviceAvailabilityDto.IsAvailable
            };

            _context.ServiceAvailabilities.Add(serviceAvailability);
            await _context.SaveChangesAsync();
            return Ok(serviceAvailability);
        }

        [HttpGet("{serviceCenterId}")]
        public IActionResult GetServiceAvailabilities(int serviceCenterId, [FromQuery] int weekNumber)
        {
            var availabilities = _context.ServiceAvailabilities
                .Where(sa => sa.ServiceCenterId == serviceCenterId && sa.WeekNumber == weekNumber)
                .ToList();
            return Ok(availabilities);
        }

        [HttpGet("{serviceCenterId}/{serviceId}")]
        public IActionResult GetServiceAvailability(int serviceCenterId, int serviceId, [FromQuery] int weekNumber, [FromQuery] string? day)
        {
            var query = _context.ServiceAvailabilities
                .Where(sa => sa.ServiceCenterId == serviceCenterId && 
                           sa.ServiceId == serviceId && 
                           sa.WeekNumber == weekNumber);

            if (!string.IsNullOrEmpty(day))
            {
                query = query.Where(sa => sa.Day == day);
            }

            var availabilities = query.ToList();
            return Ok(availabilities);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceAvailability(int id, [FromBody] UpdateServiceAvailabilityDTO updateDto)
        {
            var existingAvailability = await _context.ServiceAvailabilities.FindAsync(id);
            if (existingAvailability == null)
            {
                return NotFound("Service availability not found.");
            }

            // Check if another availability with the same details already exists (excluding current one)
            bool duplicateExists = _context.ServiceAvailabilities.Any(sa =>
                sa.Id != id &&
                sa.ServiceCenterId == updateDto.ServiceCenterId &&
                sa.ServiceId == updateDto.ServiceId &&
                sa.WeekNumber == updateDto.WeekNumber &&
                sa.Day == updateDto.Day);

            if (duplicateExists)
            {
                return BadRequest("Duplicate service availability entry.");
            }

            existingAvailability.ServiceCenterId = updateDto.ServiceCenterId;
            existingAvailability.ServiceId = updateDto.ServiceId;
            existingAvailability.WeekNumber = updateDto.WeekNumber;
            existingAvailability.Day = updateDto.Day;
            existingAvailability.IsAvailable = updateDto.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(existingAvailability);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceAvailability(int id)
        {
            var availability = await _context.ServiceAvailabilities.FindAsync(id);
            if (availability == null)
            {
                return NotFound("Service availability not found.");
            }

            _context.ServiceAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Service availability deleted successfully." });
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteServiceAvailabilities([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest("No IDs provided.");
            }

            var availabilities = await _context.ServiceAvailabilities
                .Where(sa => ids.Contains(sa.Id))
                .ToListAsync();

            if (availabilities.Count == 0)
            {
                return NotFound("No service availabilities found with the provided IDs.");
            }

            _context.ServiceAvailabilities.RemoveRange(availabilities);
            await _context.SaveChangesAsync();
            
            return Ok(new { message = $"{availabilities.Count} service availabilities deleted successfully." });
        }
    }
}
