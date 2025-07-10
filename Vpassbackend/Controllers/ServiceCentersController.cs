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
using Microsoft.Extensions.Logging;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceCentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ServiceCentersController> _logger;

        public ServiceCentersController(ApplicationDbContext context, ILogger<ServiceCentersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/ServiceCenters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCenterResponseDTO>>> GetServiceCenters()
        {
            try
            {
                // Temporary workaround for missing columns
                bool columnsExist = await CheckLatLongColumnsExist();
                if (!columnsExist)
                {
                    _logger.LogWarning("Latitude and Longitude columns not found in database. Using SQL-free approach.");
                    return await GetServiceCentersWithoutLatLong();
                }

                var serviceCenters = await _context.ServiceCenters
                    .Include(sc => sc.Services)
                    .ToListAsync();

                var serviceCenterDTOs = serviceCenters.Select(sc => new ServiceCenterResponseDTO
                {
                    ServiceCenterId = sc.Station_id,
                    OwnerName = sc.OwnerName,
                    VATNumber = sc.VATNumber ?? string.Empty,
                    RegisterationNumber = sc.RegisterationNumber ?? string.Empty,
                    StationName = sc.Station_name ?? string.Empty,
                    Email = sc.Email ?? string.Empty,
                    Telephone = sc.Telephone ?? string.Empty,
                    Address = sc.Address ?? string.Empty,
                    StationStatus = sc.Station_status ?? string.Empty,
                    Latitude = sc.Latitude,
                    Longitude = sc.Longitude,
                    ServiceCount = sc.Services?.Count ?? 0
                }).ToList();

                return serviceCenterDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service centers");

                // Fallback to alternative method if there's an error related to missing columns
                if (ex.Message.Contains("Invalid column name 'Latitude'") ||
                    ex.Message.Contains("Invalid column name 'Longitude'"))
                {
                    _logger.LogInformation("Falling back to alternative method due to missing columns");
                    return await GetServiceCentersWithoutLatLong();
                }

                return StatusCode(500, new { message = "An error occurred while retrieving service centers", error = ex.Message });
            }
        }

        // Alternative method that doesn't rely on Latitude/Longitude columns
        private async Task<ActionResult<IEnumerable<ServiceCenterResponseDTO>>> GetServiceCentersWithoutLatLong()
        {
            try
            {
                // Use raw SQL that only selects columns we know exist
                var serviceCenters = await _context.ServiceCenters
                    .FromSqlRaw(@"
                        SELECT [Station_id], [OwnerName], [VATNumber], [RegisterationNumber], 
                               [Station_name], [Email], [Telephone], [Address], [Station_status] 
                        FROM [ServiceCenters]")
                    .ToListAsync();

                // Handle services manually to avoid the issue with complex queries
                var serviceCenterIds = serviceCenters.Select(sc => sc.Station_id).ToList();
                var servicesGrouped = await _context.Services
                    .Where(s => serviceCenterIds.Contains(s.Station_id))
                    .GroupBy(s => s.Station_id)
                    .Select(g => new { ServiceCenterId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.ServiceCenterId, g => g.Count);

                var serviceCenterDTOs = serviceCenters.Select(sc => new ServiceCenterResponseDTO
                {
                    ServiceCenterId = sc.Station_id,
                    OwnerName = sc.OwnerName,
                    VATNumber = sc.VATNumber ?? string.Empty,
                    RegisterationNumber = sc.RegisterationNumber ?? string.Empty,
                    StationName = sc.Station_name ?? string.Empty,
                    Email = sc.Email ?? string.Empty,
                    Telephone = sc.Telephone ?? string.Empty,
                    Address = sc.Address ?? string.Empty,
                    StationStatus = sc.Station_status ?? string.Empty,
                    // Set default values for the missing columns
                    Latitude = null,
                    Longitude = null,
                    ServiceCount = servicesGrouped.ContainsKey(sc.Station_id) ? servicesGrouped[sc.Station_id] : 0
                }).ToList();

                return serviceCenterDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in fallback method GetServiceCentersWithoutLatLong");
                return StatusCode(500, new { message = "An error occurred in the fallback method", error = ex.Message });
            }
        }

        // Helper method to check if Latitude and Longitude columns exist
        private async Task<bool> CheckLatLongColumnsExist()
        {
            try
            {
                // Try to execute a query that checks for the existence of the columns
                var result = await _context.Database.ExecuteSqlRawAsync(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'ServiceCenters' 
                    AND COLUMN_NAME IN ('Latitude', 'Longitude')");

                // If we got here, the query executed successfully
                return true;
            }
            catch
            {
                // If there was an error, assume the columns don't exist
                return false;
            }
        }

        // GET: api/ServiceCenters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceCenterResponseDTO>> GetServiceCenter(int id)
        {
            try
            {
                // Check if columns exist first
                bool columnsExist = await CheckLatLongColumnsExist();

                if (!columnsExist)
                {
                    _logger.LogWarning($"Latitude and Longitude columns not found in database. Using SQL-free approach for ID {id}");
                    return await GetServiceCenterByIdWithoutLatLong(id);
                }

                var serviceCenter = await _context.ServiceCenters
                    .Include(sc => sc.Services)
                    .FirstOrDefaultAsync(sc => sc.Station_id == id);

                if (serviceCenter == null)
                {
                    return NotFound(new { message = $"Service center with ID {id} not found" });
                }

                var serviceCenterDTO = new ServiceCenterResponseDTO
                {
                    ServiceCenterId = serviceCenter.Station_id,
                    OwnerName = serviceCenter.OwnerName,
                    VATNumber = serviceCenter.VATNumber ?? string.Empty,
                    RegisterationNumber = serviceCenter.RegisterationNumber ?? string.Empty,
                    StationName = serviceCenter.Station_name ?? string.Empty,
                    Email = serviceCenter.Email ?? string.Empty,
                    Telephone = serviceCenter.Telephone ?? string.Empty,
                    Address = serviceCenter.Address ?? string.Empty,
                    StationStatus = serviceCenter.Station_status ?? string.Empty,
                    Latitude = serviceCenter.Latitude,
                    Longitude = serviceCenter.Longitude,
                    ServiceCount = serviceCenter.Services?.Count ?? 0
                };

                return serviceCenterDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service center with ID {id}");

                // Fallback to alternative method if there's an error related to missing columns
                if (ex.Message.Contains("Invalid column name 'Latitude'") ||
                    ex.Message.Contains("Invalid column name 'Longitude'"))
                {
                    _logger.LogInformation($"Falling back to alternative method due to missing columns for ID {id}");
                    return await GetServiceCenterByIdWithoutLatLong(id);
                }

                return StatusCode(500, new { message = $"An error occurred while retrieving service center with ID {id}", error = ex.Message });
            }
        }

        // Alternative method to get a service center by ID without relying on Latitude/Longitude columns
        private async Task<ActionResult<ServiceCenterResponseDTO>> GetServiceCenterByIdWithoutLatLong(int id)
        {
            try
            {
                // Use raw SQL that only selects columns we know exist with parameterized query to prevent SQL injection
                var serviceCenter = await _context.ServiceCenters
                    .FromSqlRaw(@"
                        SELECT [Station_id], [OwnerName], [VATNumber], [RegisterationNumber], 
                               [Station_name], [Email], [Telephone], [Address], [Station_status] 
                        FROM [ServiceCenters]
                        WHERE [Station_id] = {0}", id)
                    .FirstOrDefaultAsync();

                if (serviceCenter == null)
                {
                    return NotFound(new { message = $"Service center with ID {id} not found" });
                }

                // Get service count separately
                var serviceCount = await _context.Services
                    .CountAsync(s => s.Station_id == id);

                var serviceCenterDTO = new ServiceCenterResponseDTO
                {
                    ServiceCenterId = serviceCenter.Station_id,
                    OwnerName = serviceCenter.OwnerName,
                    VATNumber = serviceCenter.VATNumber ?? string.Empty,
                    RegisterationNumber = serviceCenter.RegisterationNumber ?? string.Empty,
                    StationName = serviceCenter.Station_name ?? string.Empty,
                    Email = serviceCenter.Email ?? string.Empty,
                    Telephone = serviceCenter.Telephone ?? string.Empty,
                    Address = serviceCenter.Address ?? string.Empty,
                    StationStatus = serviceCenter.Station_status ?? string.Empty,
                    // Set default values for the missing columns
                    Latitude = null,
                    Longitude = null,
                    ServiceCount = serviceCount
                };

                return serviceCenterDTO;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in fallback method GetServiceCenterByIdWithoutLatLong for ID {id}");
                return StatusCode(500, new { message = $"An error occurred in the fallback method for ID {id}", error = ex.Message });
            }
        }

        // POST: api/ServiceCenters
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ServiceCenterResponseDTO>> CreateServiceCenter(ServiceCenterCreateDTO dto)
        {
            // Check if a service center with the same email exists
            if (await _context.ServiceCenters.AnyAsync(sc => sc.Email == dto.Email))
            {
                return BadRequest(new { message = "A service center with this email already exists" });
            }

            var serviceCenter = new ServiceCenter
            {
                OwnerName = dto.OwnerName,
                VATNumber = dto.VATNumber,
                RegisterationNumber = dto.RegisterationNumber,
                Station_name = dto.StationName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Address = dto.Address,
                Station_status = "Active",
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Services = new List<Service>(),
                CheckInPoints = new List<ServiceCenterCheckInPoint>(),
                Appointments = new List<Appointment>()
            };

            _context.ServiceCenters.Add(serviceCenter);
            await _context.SaveChangesAsync();

            var response = new ServiceCenterResponseDTO
            {
                ServiceCenterId = serviceCenter.Station_id,
                OwnerName = serviceCenter.OwnerName,
                VATNumber = serviceCenter.VATNumber,
                RegisterationNumber = serviceCenter.RegisterationNumber,
                StationName = serviceCenter.Station_name,
                Email = serviceCenter.Email,
                Telephone = serviceCenter.Telephone,
                Address = serviceCenter.Address,
                StationStatus = serviceCenter.Station_status,
                Latitude = serviceCenter.Latitude,
                Longitude = serviceCenter.Longitude,
                ServiceCount = 0
            };

            return CreatedAtAction(nameof(GetServiceCenter), new { id = serviceCenter.Station_id }, response);
        }

        // PUT: api/ServiceCenters/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> UpdateServiceCenter(int id, ServiceCenterUpdateDTO dto)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return NotFound();
            }

            // Check for email uniqueness only if email is being changed
            if (dto.Email != null && dto.Email != serviceCenter.Email)
            {
                if (await _context.ServiceCenters.AnyAsync(sc => sc.Email == dto.Email))
                {
                    return BadRequest(new { message = "A service center with this email already exists" });
                }
            }

            // Update only non-null values
            if (dto.OwnerName != null) serviceCenter.OwnerName = dto.OwnerName;
            if (dto.VATNumber != null) serviceCenter.VATNumber = dto.VATNumber;
            if (dto.RegisterationNumber != null) serviceCenter.RegisterationNumber = dto.RegisterationNumber;
            if (dto.StationName != null) serviceCenter.Station_name = dto.StationName;
            if (dto.Email != null) serviceCenter.Email = dto.Email;
            if (dto.Telephone != null) serviceCenter.Telephone = dto.Telephone;
            if (dto.Address != null) serviceCenter.Address = dto.Address;
            if (dto.StationStatus != null) serviceCenter.Station_status = dto.StationStatus;
            if (dto.Latitude.HasValue) serviceCenter.Latitude = dto.Latitude;
            if (dto.Longitude.HasValue) serviceCenter.Longitude = dto.Longitude;

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

            // Validate status
            if (status != "Active" && status != "Inactive" && status != "Suspended")
            {
                return BadRequest(new { message = "Invalid status. Allowed values: Active, Inactive, Suspended" });
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

            // Check if there are any appointments for this service center
            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceCenterId == id);
            if (hasAppointments)
            {
                // Instead of deleting, mark as inactive
                serviceCenter.Station_status = "Inactive";
                await _context.SaveChangesAsync();
                return Ok(new { message = "Service center has existing appointments. Status set to Inactive instead of deleting." });
            }

            // Check if there are any service history records for this service center
            var hasServiceHistory = await _context.VehicleServiceHistory.AnyAsync(vsh => vsh.ServiceCenterId == id);
            if (hasServiceHistory)
            {
                // Instead of deleting, mark as inactive
                serviceCenter.Station_status = "Inactive";
                await _context.SaveChangesAsync();
                return Ok(new { message = "Service center has existing service history records. Status set to Inactive instead of deleting." });
            }

            // Delete all services associated with this service center
            var services = await _context.Services.Where(s => s.Station_id == id).ToListAsync();
            _context.Services.RemoveRange(services);

            // Delete all check-in points associated with this service center
            var checkInPoints = await _context.ServiceCenterCheckInPoints.Where(cip => cip.ServiceCenterId == id).ToListAsync();
            _context.ServiceCenterCheckInPoints.RemoveRange(checkInPoints);

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
