using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FuelUsagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FuelUsagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddFuelUsage([FromBody] FuelUsage usage)
        {
            _context.FuelUsages.Add(usage);
            await _context.SaveChangesAsync();
            return Ok(usage);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetMonthlySummary(int userId)
        {
            var summary = await _context.FuelUsages
                .Where(f => f.UserId == userId)
                .GroupBy(f => new { f.Date.Year, f.Date.Month })
                .Select(g => new {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    TotalAmount = g.Sum(f => f.Amount)
                })
                .ToListAsync();

            return Ok(summary);
        }
    }
}
