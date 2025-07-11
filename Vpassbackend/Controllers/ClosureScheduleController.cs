using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.Models;

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
    }
}
