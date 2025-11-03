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
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServiceController(ApplicationDbContext context) => _context = context;

        [HttpGet("{serviceCenterId}")]
        public IActionResult GetServices(int serviceCenterId, [FromQuery] DateTime date)
        {
            var weekNumber = GetWeekNumber(date);
            var day = date.DayOfWeek.ToString();

            var services = _context.ServiceCenterServices
                .Where(s => s.Station_id == serviceCenterId)
                .Select(s => new
                {
                    s.ServiceId,
                    s.Service.ServiceName,
                    s.ServiceCenterServiceId,
                    IsAvailable = s.IsAvailable && !_context.ServiceAvailabilities.Any(a =>
                        a.ServiceCenterId == serviceCenterId &&
                        a.ServiceId == s.ServiceId &&
                        a.Date.Date == date.Date &&
                        !a.IsAvailable)
                }).ToList();

            // Check closure schedules for this date
            bool isClosed = _context.ClosureSchedules
                .Any(cs => cs.ServiceCenterId == serviceCenterId &&
                           cs.ClosureDate.Date == date.Date);

            if (isClosed)
            {
                // If service center is closed, mark all services as unavailable
                services = services.Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName,
                    s.ServiceCenterServiceId,
                    IsAvailable = false
                }).ToList();
            }

            return Ok(services);
        }

        [HttpPost("SetAvailability")]
        public async Task<IActionResult> SetAvailability([FromBody] ServiceAvailability availability)
        {
            // Reset the ID to 0 to let Entity Framework generate it
            availability.Id = 0;

            var existing = _context.ServiceAvailabilities.FirstOrDefault(a =>
                a.ServiceCenterId == availability.ServiceCenterId &&
                a.ServiceId == availability.ServiceId &&
                a.Date.Date == availability.Date.Date);

            if (existing != null)
            {
                existing.IsAvailable = availability.IsAvailable;
            }
            else
            {
                _context.ServiceAvailabilities.Add(availability);
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("{serviceCenterId}/available")]
        public IActionResult GetAvailableServices(int serviceCenterId, [FromQuery] DateTime date)
        {
            var weekNumber = GetWeekNumber(date);
            var day = date.DayOfWeek.ToString();

            // Check if the service center is closed on this date
            bool isClosed = _context.ClosureSchedules
                .Any(cs => cs.ServiceCenterId == serviceCenterId && cs.ClosureDate.Date == date.Date);

            if (isClosed)
                return Ok(new List<object>()); // Service center is closed

            // Get available services considering both ServiceCenterServices.IsAvailable and ServiceAvailabilities
            var availableServices = (from scs in _context.ServiceCenterServices
                                     join s in _context.Services on scs.ServiceId equals s.ServiceId
                                     where scs.Station_id == serviceCenterId
                                         && scs.IsAvailable // Check if service is available at service center level
                                     select new
                                     {
                                         scs.ServiceId,
                                         ServiceName = s.ServiceName,
                                         IsAvailable = scs.IsAvailable && !_context.ServiceAvailabilities.Any(sa =>
                                             sa.ServiceCenterId == serviceCenterId &&
                                             sa.ServiceId == scs.ServiceId &&
                                             sa.Date.Date == date.Date &&
                                             !sa.IsAvailable)
                                     })
                                     .Where(s => s.IsAvailable)
                                     .Select(s => new
                                     {
                                         s.ServiceId,
                                         s.ServiceName
                                     })
                                     .ToList();

            return Ok(availableServices);
        }

        // ========== SERVICE AVAILABILITY MANAGEMENT ==========

        [HttpPost("availability")]
        public async Task<IActionResult> AddServiceAvailability([FromBody] ServiceAvailabilityDTO serviceAvailabilityDto)
        {
            bool exists = _context.ServiceAvailabilities.Any(sa =>
                sa.ServiceCenterId == serviceAvailabilityDto.ServiceCenterId &&
                sa.ServiceId == serviceAvailabilityDto.ServiceId &&
                sa.Date.Date == serviceAvailabilityDto.Date.Date);

            if (exists)
                return BadRequest("Duplicate service availability entry.");

            var serviceAvailability = new ServiceAvailability
            {
                ServiceCenterId = serviceAvailabilityDto.ServiceCenterId,
                ServiceId = serviceAvailabilityDto.ServiceId,
                Date = serviceAvailabilityDto.Date,
                IsAvailable = serviceAvailabilityDto.IsAvailable
            };

            _context.ServiceAvailabilities.Add(serviceAvailability);
            await _context.SaveChangesAsync();
            return Ok(serviceAvailability);
        }

        [HttpGet("{serviceCenterId}/availability")]
        public IActionResult GetServiceAvailabilities(int serviceCenterId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var query = _context.ServiceAvailabilities
                .Where(sa => sa.ServiceCenterId == serviceCenterId);

            if (startDate.HasValue)
            {
                query = query.Where(sa => sa.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(sa => sa.Date <= endDate.Value.Date);
            }

            var availabilities = query.ToList();
            return Ok(availabilities);
        }

        [HttpGet("{serviceCenterId}/availability/{serviceId}")]
        public IActionResult GetServiceAvailability(int serviceCenterId, int serviceId, [FromQuery] DateTime? date)
        {
            var query = _context.ServiceAvailabilities
                .Where(sa => sa.ServiceCenterId == serviceCenterId &&
                           sa.ServiceId == serviceId);

            if (date.HasValue)
            {
                query = query.Where(sa => sa.Date.Date == date.Value.Date);
            }

            var availabilities = query.ToList();
            return Ok(availabilities);
        }

        [HttpPut("availability/{id}")]
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
                sa.Date.Date == updateDto.Date.Date);

            if (duplicateExists)
            {
                return BadRequest("Duplicate service availability entry.");
            }

            existingAvailability.ServiceCenterId = updateDto.ServiceCenterId;
            existingAvailability.ServiceId = updateDto.ServiceId;
            existingAvailability.Date = updateDto.Date;
            existingAvailability.IsAvailable = updateDto.IsAvailable;

            await _context.SaveChangesAsync();
            return Ok(existingAvailability);
        }

        [HttpDelete("availability/{id}")]
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

        [HttpDelete("availability/bulk")]
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

        private int GetWeekNumber(DateTime date)
        {
            var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
            return calendar.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
    }
}