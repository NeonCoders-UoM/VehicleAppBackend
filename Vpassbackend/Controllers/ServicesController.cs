using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServices()
        {
            var services = await _context.Services
                .Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    BasePrice = s.BasePrice,
                    Category = s.Category
                })
                .ToListAsync();

            return services;
        }

        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDTO>> GetService(int id)
        {
            var service = await _context.Services
                .Where(s => s.ServiceId == id)
                .Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    BasePrice = s.BasePrice,
                    Category = s.Category
                })
                .FirstOrDefaultAsync();

            if (service == null)
            {
                return NotFound();
            }

            return service;
        }

        // GET: api/Services/ServiceCenter/5
        [HttpGet("ServiceCenter/{stationId}")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByServiceCenter(int stationId)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(stationId);

            if (serviceCenter == null)
            {
                return NotFound("Service center not found");
            }

            var services = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Where(scs => scs.Station_id == stationId)
                .Select(scs => new ServiceDTO
                {
                    ServiceId = scs.Service.ServiceId,
                    ServiceName = scs.Service.ServiceName,
                    Description = scs.Service.Description,
                    BasePrice = scs.CustomPrice ?? scs.Service.BasePrice,
                    Category = scs.Service.Category,
                    Station_id = scs.Station_id
                })
                .ToListAsync();

            return services;
        }

        // GET: api/Services/Category/Maintenance
        [HttpGet("Category/{category}")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByCategory(string category)
        {
            var services = await _context.Services
                .Where(s => s.Category != null && s.Category.ToLower() == category.ToLower())
                .Select(s => new ServiceDTO
                {
                    ServiceId = s.ServiceId,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    BasePrice = s.BasePrice,
                    Category = s.Category
                })
                .ToListAsync();

            return services;
        }

        // POST: api/Services
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ServiceDTO>> CreateService(CreateServiceDTO createServiceDTO)
        {
            var service = new Service
            {
                ServiceName = createServiceDTO.ServiceName,
                Description = createServiceDTO.Description,
                BasePrice = createServiceDTO.BasePrice,
                Category = createServiceDTO.Category
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            var serviceDTO = new ServiceDTO
            {
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                Description = service.Description,
                BasePrice = service.BasePrice,
                Category = service.Category
            };

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, serviceDTO);
        }

        // PUT: api/Services/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateService(int id, UpdateServiceDTO updateServiceDTO)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            // Update only the provided fields
            if (updateServiceDTO.ServiceName != null)
                service.ServiceName = updateServiceDTO.ServiceName;

            if (updateServiceDTO.Description != null)
                service.Description = updateServiceDTO.Description;

            if (updateServiceDTO.BasePrice.HasValue)
                service.BasePrice = updateServiceDTO.BasePrice;



            if (updateServiceDTO.Category != null)
                service.Category = updateServiceDTO.Category;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Services/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            // Check if this service has any appointments
            bool hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceId == id);
            if (hasAppointments)
            {
                return BadRequest("Cannot delete service as it has associated appointments");
            }

            // Check if this service is offered by any service centers
            bool isOfferedByCenters = await _context.ServiceCenterServices.AnyAsync(scs => scs.ServiceId == id);
            if (isOfferedByCenters)
            {
                return BadRequest("Cannot delete service as it is offered by one or more service centers");
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}
