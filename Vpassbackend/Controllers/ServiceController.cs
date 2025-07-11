using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ServiceController(ApplicationDbContext context) => _context = context;

        [HttpGet("{serviceCenterId}")]
        public IActionResult GetServices(int serviceCenterId, [FromQuery] int weekNumber)
        {
            var services = _context.ServiceCenterServices
                .Where(s => s.Station_id == serviceCenterId)
                .Select(s => new {
                    s.ServiceId,
                    s.Service.ServiceName,
                    IsAvailable = !_context.ServiceAvailabilities.Any(a =>
                        a.ServiceCenterId == serviceCenterId &&
                        a.ServiceId == s.ServiceId &&
                        a.WeekNumber == weekNumber &&
                        !a.IsAvailable)
                }).ToList();
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
                a.WeekNumber == availability.WeekNumber &&
                a.Day == availability.Day);

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
        public IActionResult GetAvailableServices(int serviceCenterId, [FromQuery] int weekNumber, [FromQuery] string day)
        {
            // Check if the service center is closed on this week and day
            bool isClosed = _context.ClosureSchedules
                .Any(cs => cs.ServiceCenterId == serviceCenterId && cs.WeekNumber == weekNumber && cs.Day == day);

            if (isClosed)
                return Ok(new List<object>()); // Service center is closed

            var availableServices = (from sa in _context.ServiceAvailabilities
                                     join s in _context.Services on sa.ServiceId equals s.ServiceId
                                     where sa.ServiceCenterId == serviceCenterId
                                         && sa.WeekNumber == weekNumber
                                         && sa.Day == day
                                         && sa.IsAvailable
                                     select new {
                                         sa.ServiceId,
                                         ServiceName = s.ServiceName
                                     }).ToList();

            return Ok(availableServices);
        }
    }
}
