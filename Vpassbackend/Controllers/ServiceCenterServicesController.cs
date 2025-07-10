using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/ServiceCenters/{serviceCenterId}/Services")]
    public class ServiceCenterServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceCenterServicesController> _logger;

        public ServiceCenterServicesController(ApplicationDbContext context, ILogger<ServiceCenterServicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServiceCenters/5/Services
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServices(int serviceCenterId)
        {
            try
            {
                if (!await ServiceCenterExists(serviceCenterId))
                {
                    return NotFound(new { message = $"Service center with ID {serviceCenterId} not found" });
                }

                var services = await _context.Services
                    .Where(s => s.Station_id == serviceCenterId)
                    .Select(s => new ServiceDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        Description = s.Description ?? string.Empty,
                        BasePrice = s.BasePrice,
                        StationId = s.Station_id
                    })
                    .ToListAsync();

                return services;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting services for service center ID {serviceCenterId}");
                return StatusCode(500, new { message = $"An error occurred while retrieving services for service center ID {serviceCenterId}", error = ex.Message });
            }
        }

        // GET: api/ServiceCenters/5/Services/3
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDTO>> GetService(int serviceCenterId, int id)
        {
            try
            {
                if (!await ServiceCenterExists(serviceCenterId))
                {
                    return NotFound(new { message = $"Service center with ID {serviceCenterId} not found" });
                }

                var service = await _context.Services
                    .Where(s => s.Station_id == serviceCenterId && s.ServiceId == id)
                    .Select(s => new ServiceDTO
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName,
                        Description = s.Description ?? string.Empty,
                        BasePrice = s.BasePrice,
                        StationId = s.Station_id
                    })
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return NotFound(new { message = $"Service with ID {id} not found in service center with ID {serviceCenterId}" });
                }

                return service;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service with ID {id} for service center ID {serviceCenterId}");
                return StatusCode(500, new { message = $"An error occurred while retrieving service with ID {id} for service center ID {serviceCenterId}", error = ex.Message });
            }
        }

        // POST: api/ServiceCenters/5/Services
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<ActionResult<ServiceDTO>> CreateService(int serviceCenterId, ServiceDTO dto)
        {
            try
            {
                if (!await ServiceCenterExists(serviceCenterId))
                {
                    return NotFound(new { message = $"Service center with ID {serviceCenterId} not found" });
                }

                // Get the service center
                var serviceCenter = await _context.ServiceCenters.FindAsync(serviceCenterId);
                if (serviceCenter == null)
                {
                    return NotFound(new { message = $"Service center with ID {serviceCenterId} not found" });
                }

                var service = new Service
                {
                    ServiceName = dto.ServiceName,
                    Description = dto.Description,
                    BasePrice = dto.BasePrice,
                    Station_id = serviceCenterId,
                    ServiceCenter = serviceCenter
                };

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                dto.ServiceId = service.ServiceId;
                dto.StationId = serviceCenterId;

                return CreatedAtAction(nameof(GetService), new { serviceCenterId = serviceCenterId, id = service.ServiceId }, dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating service for service center ID {serviceCenterId}");
                return StatusCode(500, new { message = $"An error occurred while creating service for service center ID {serviceCenterId}", error = ex.Message });
            }
        }

        // PUT: api/ServiceCenters/5/Services/3
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> UpdateService(int serviceCenterId, int id, ServiceDTO dto)
        {
            if (id != dto.ServiceId)
            {
                return BadRequest(new { message = "Service ID mismatch" });
            }

            if (!await ServiceCenterExists(serviceCenterId))
            {
                return NotFound(new { message = "Service center not found" });
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.Station_id == serviceCenterId);

            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            service.ServiceName = dto.ServiceName;
            service.Description = dto.Description;
            service.BasePrice = dto.BasePrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExists(id))
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

        // DELETE: api/ServiceCenters/5/Services/3
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> DeleteService(int serviceCenterId, int id)
        {
            if (!await ServiceCenterExists(serviceCenterId))
            {
                return NotFound(new { message = "Service center not found" });
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.Station_id == serviceCenterId);

            if (service == null)
            {
                return NotFound(new { message = "Service not found" });
            }

            // Check if this service is used in any appointments
            var isUsedInAppointments = await _context.AppointmentServices.AnyAsync(asvc => asvc.ServiceId == id);
            if (isUsedInAppointments)
            {
                return BadRequest(new { message = "Cannot delete service as it is associated with appointments" });
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ServiceCenterExists(int id)
        {
            return await _context.ServiceCenters.AnyAsync(e => e.Station_id == id);
        }

        private async Task<bool> ServiceExists(int id)
        {
            return await _context.Services.AnyAsync(e => e.ServiceId == id);
        }
    }
}
