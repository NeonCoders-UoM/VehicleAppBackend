using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Dtos;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyCallCenterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmergencyCallCenterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/EmergencyCallCenter
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> CreateCenter(EmergencyCallCenterDto dto)
        {
            var center = new EmergencyCallCenter
            {
                Name = dto.Name,
                Address = dto.Address,
                RegistrationNumber = dto.RegistrationNumber,
                PhoneNumber = dto.PhoneNumber
            };

            _context.EmergencyCallCenters.Add(center);
            await _context.SaveChangesAsync();

            return Ok(center);
        }

        // GET: api/EmergencyCallCenter
        [HttpGet]
        public async Task<IActionResult> GetAllCenters()
        {
            var centers = await _context.EmergencyCallCenters.ToListAsync();
            return Ok(centers);
        }



        // PUT: api/EmergencyCallCenter/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateCenter(int id, EmergencyCallCenterDto dto)
        {
            var center = await _context.EmergencyCallCenters.FindAsync(id);
            if (center == null)
                return NotFound();

            center.Name = dto.Name;
            center.Address = dto.Address;
            center.RegistrationNumber = dto.RegistrationNumber;
            center.PhoneNumber = dto.PhoneNumber;

            await _context.SaveChangesAsync();

            return Ok(center);
        }

        // DELETE: api/EmergencyCallCenter/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteCenter(int id)
        {
            var center = await _context.EmergencyCallCenters.FindAsync(id);
            if (center == null)
                return NotFound();

            _context.EmergencyCallCenters.Remove(center);
            await _context.SaveChangesAsync();

            return Ok($"Center with ID {id} deleted.");
        }
    }
}
