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
    public class ServiceCentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceCentersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceCenters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCenterDTO>>> GetServiceCenters()
        {
            var serviceCenters = await _context.ServiceCenters
                .Select(sc => new ServiceCenterDTO
                {
                    Station_id = sc.Station_id,
                    OwnerName = sc.OwnerName,
                    VATNumber = sc.VATNumber,
                    RegisterationNumber = sc.RegisterationNumber,
                    Station_name = sc.Station_name,
                    Email = sc.Email,
                    Telephone = sc.Telephone,
                    Address = sc.Address,
                    Station_status = sc.Station_status
                })
                .ToListAsync();

            return serviceCenters;
        }

        // GET: api/ServiceCenters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceCenterDTO>> GetServiceCenter(int id)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);

            if (serviceCenter == null)
            {
                return NotFound();
            }

            var serviceCenterDTO = new ServiceCenterDTO
            {
                Station_id = serviceCenter.Station_id,
                OwnerName = serviceCenter.OwnerName,
                VATNumber = serviceCenter.VATNumber,
                RegisterationNumber = serviceCenter.RegisterationNumber,
                Station_name = serviceCenter.Station_name,
                Email = serviceCenter.Email,
                Telephone = serviceCenter.Telephone,
                Address = serviceCenter.Address,
                Station_status = serviceCenter.Station_status
            };

            return serviceCenterDTO;
        }

        // GET: api/ServiceCenters/status/active
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<ServiceCenterDTO>>> GetServiceCentersByStatus(string status)
        {
            var serviceCenters = await _context.ServiceCenters
                .Where(sc => sc.Station_status != null && sc.Station_status.ToLower() == status.ToLower())
                .Select(sc => new ServiceCenterDTO
                {
                    Station_id = sc.Station_id,
                    OwnerName = sc.OwnerName,
                    VATNumber = sc.VATNumber,
                    RegisterationNumber = sc.RegisterationNumber,
                    Station_name = sc.Station_name,
                    Email = sc.Email,
                    Telephone = sc.Telephone,
                    Address = sc.Address,
                    Station_status = sc.Station_status
                })
                .ToListAsync();

            return serviceCenters;
        }

        // POST: api/ServiceCenters
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ServiceCenterDTO>> CreateServiceCenter(CreateServiceCenterDTO createServiceCenterDTO)
        {
            var serviceCenter = new ServiceCenter
            {
                OwnerName = createServiceCenterDTO.OwnerName,
                VATNumber = createServiceCenterDTO.VATNumber,
                RegisterationNumber = createServiceCenterDTO.RegisterationNumber,
                Station_name = createServiceCenterDTO.Station_name,
                Email = createServiceCenterDTO.Email,
                Telephone = createServiceCenterDTO.Telephone,
                Address = createServiceCenterDTO.Address,
                Station_status = createServiceCenterDTO.Station_status
            };

            _context.ServiceCenters.Add(serviceCenter);
            await _context.SaveChangesAsync();

            var serviceCenterDTO = new ServiceCenterDTO
            {
                Station_id = serviceCenter.Station_id,
                OwnerName = serviceCenter.OwnerName,
                VATNumber = serviceCenter.VATNumber,
                RegisterationNumber = serviceCenter.RegisterationNumber,
                Station_name = serviceCenter.Station_name,
                Email = serviceCenter.Email,
                Telephone = serviceCenter.Telephone,
                Address = serviceCenter.Address,
                Station_status = serviceCenter.Station_status
            };

            return CreatedAtAction(nameof(GetServiceCenter), new { id = serviceCenter.Station_id }, serviceCenterDTO);
        }

        // PUT: api/ServiceCenters/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateServiceCenter(int id, UpdateServiceCenterDTO updateServiceCenterDTO)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return NotFound();
            }

            // Update only the provided fields
            if (updateServiceCenterDTO.OwnerName != null)
                serviceCenter.OwnerName = updateServiceCenterDTO.OwnerName;

            if (updateServiceCenterDTO.VATNumber != null)
                serviceCenter.VATNumber = updateServiceCenterDTO.VATNumber;

            if (updateServiceCenterDTO.RegisterationNumber != null)
                serviceCenter.RegisterationNumber = updateServiceCenterDTO.RegisterationNumber;

            if (updateServiceCenterDTO.Station_name != null)
                serviceCenter.Station_name = updateServiceCenterDTO.Station_name;

            if (updateServiceCenterDTO.Email != null)
                serviceCenter.Email = updateServiceCenterDTO.Email;

            if (updateServiceCenterDTO.Telephone != null)
                serviceCenter.Telephone = updateServiceCenterDTO.Telephone;

            if (updateServiceCenterDTO.Address != null)
                serviceCenter.Address = updateServiceCenterDTO.Address;

            if (updateServiceCenterDTO.Station_status != null)
                serviceCenter.Station_status = updateServiceCenterDTO.Station_status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCenterExists(id))
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

        // PATCH: api/ServiceCenters/5/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateServiceCenterStatus(int id, [FromBody] string status)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return NotFound();
            }

            serviceCenter.Station_status = status;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCenterExists(id))
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

        // DELETE: api/ServiceCenters/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteServiceCenter(int id)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return NotFound();
            }

            _context.ServiceCenters.Remove(serviceCenter);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceCenterExists(int id)
        {
            return _context.ServiceCenters.Any(e => e.Station_id == id);
        }
    }
}
