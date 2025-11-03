using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ServiceCenterServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoyaltyPointsService _loyaltyPointsService;

        public ServiceCenterServicesController(ApplicationDbContext context, ILoyaltyPointsService loyaltyPointsService)
        {
            _context = context;
            _loyaltyPointsService = loyaltyPointsService;
        }

        // GET: api/ServiceCenterServices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCenterServiceDTO>>> GetServiceCenterServices()
        {
            // Get user role and ID from JWT token
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            IQueryable<ServiceCenterService> query = _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package);

            // If ServiceCenterAdmin, only show their assigned service center
            if (userRole == "ServiceCenterAdmin" && !string.IsNullOrEmpty(userIdClaim))
            {
                var userId = int.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);
                if (user?.Station_id != null)
                {
                    query = query.Where(scs => scs.Station_id == user.Station_id);
                }
            }

            var serviceCenterServices = await query
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    PackageId = scs.PackageId,
                    CustomPrice = scs.CustomPrice,
                    ServiceCenterBasePrice = scs.BasePrice,
                    ServiceCenterLoyaltyPoints = scs.LoyaltyPoints,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    ServiceBasePrice = scs.Service.BasePrice,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name,
                    PackageName = scs.Package != null ? scs.Package.PackageName : null,
                    PackagePercentage = scs.Package != null ? scs.Package.Percentage : null,
                    PackageDescription = scs.Package != null ? scs.Package.Description : null
                })
                .ToListAsync();

            return serviceCenterServices;
        }

        // GET: api/ServiceCenterServices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceCenterServiceDTO>> GetServiceCenterService(int id)
        {
            var serviceCenterService = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package)
                .Where(scs => scs.ServiceCenterServiceId == id)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    PackageId = scs.PackageId,
                    CustomPrice = scs.CustomPrice,
                    ServiceCenterBasePrice = scs.BasePrice,
                    ServiceCenterLoyaltyPoints = scs.LoyaltyPoints,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    ServiceBasePrice = scs.Service.BasePrice,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name,
                    PackageName = scs.Package != null ? scs.Package.PackageName : null,
                    PackagePercentage = scs.Package != null ? scs.Package.Percentage : null,
                    PackageDescription = scs.Package != null ? scs.Package.Description : null
                })
                .FirstOrDefaultAsync();

            if (serviceCenterService == null)
            {
                return NotFound();
            }

            return serviceCenterService;
        }

        // GET: api/ServiceCenterServices/ByServiceCenter/5
        [HttpGet("ByServiceCenter/{stationId}")]
        public async Task<ActionResult<IEnumerable<ServiceCenterServiceDTO>>> GetServiceCenterServicesByStation(int stationId)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(stationId);
            if (serviceCenter == null)
            {
                return NotFound("Service center not found");
            }

            var serviceCenterServices = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package)
                .Where(scs => scs.Station_id == stationId)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    PackageId = scs.PackageId,
                    CustomPrice = scs.CustomPrice,
                    ServiceCenterBasePrice = scs.BasePrice,
                    ServiceCenterLoyaltyPoints = scs.LoyaltyPoints,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    ServiceBasePrice = scs.Service.BasePrice,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name,
                    PackageName = scs.Package != null ? scs.Package.PackageName : null,
                    PackagePercentage = scs.Package != null ? scs.Package.Percentage : null,
                    PackageDescription = scs.Package != null ? scs.Package.Description : null
                })
                .ToListAsync();

            return serviceCenterServices;
        }

        // GET: api/ServiceCenterServices/ByService/5
        [HttpGet("ByService/{serviceId}")]
        public async Task<ActionResult<IEnumerable<ServiceCenterServiceDTO>>> GetServiceCenterServicesByService(int serviceId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return NotFound("Service not found");
            }

            var serviceCenterServices = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package)
                .Where(scs => scs.ServiceId == serviceId)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    PackageId = scs.PackageId,
                    CustomPrice = scs.CustomPrice,
                    ServiceCenterBasePrice = scs.BasePrice,
                    ServiceCenterLoyaltyPoints = scs.LoyaltyPoints,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    ServiceBasePrice = scs.Service.BasePrice,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name,
                    PackageName = scs.Package != null ? scs.Package.PackageName : null,
                    PackagePercentage = scs.Package != null ? scs.Package.Percentage : null,
                    PackageDescription = scs.Package != null ? scs.Package.Description : null
                })
                .ToListAsync();

            return serviceCenterServices;
        }

        // POST: api/ServiceCenterServices
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<ActionResult<ServiceCenterServiceDTO>> CreateServiceCenterService(CreateServiceCenterServiceDTO createDto)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(createDto.Station_id);
            if (serviceCenter == null)
            {
                return BadRequest("Invalid service center ID");
            }

            var service = await _context.Services.FindAsync(createDto.ServiceId);
            if (service == null)
            {
                return BadRequest("Invalid service ID");
            }

            // Validate package if provided
            Package? package = null;
            if (createDto.PackageId.HasValue)
            {
                package = await _context.Packages.FindAsync(createDto.PackageId.Value);
                if (package == null)
                {
                    return BadRequest("Invalid package ID");
                }
                if (!package.IsActive)
                {
                    return BadRequest("Selected package is not active");
                }
            }

            // Check if the relation already exists
            bool alreadyExists = await _context.ServiceCenterServices
                .AnyAsync(scs => scs.ServiceId == createDto.ServiceId && scs.Station_id == createDto.Station_id);

            if (alreadyExists)
            {
                return BadRequest("This service is already offered by this service center");
            }

            // Calculate base price (use custom price if provided, otherwise use service base price)
            decimal basePrice = createDto.ServiceCenterBasePrice ?? createDto.CustomPrice ?? service.BasePrice ?? 0;

            // Calculate loyalty points based on package percentage
            int loyaltyPoints = _loyaltyPointsService.CalculateLoyaltyPoints(basePrice, package?.Percentage);

            var serviceCenterService = new ServiceCenterService
            {
                Station_id = createDto.Station_id,
                ServiceId = createDto.ServiceId,
                PackageId = createDto.PackageId,
                CustomPrice = createDto.CustomPrice,
                BasePrice = basePrice,
                LoyaltyPoints = loyaltyPoints,
                IsAvailable = createDto.IsAvailable,
                Notes = createDto.Notes,
                ServiceCenter = serviceCenter,
                Service = service,
                Package = package
            };

            _context.ServiceCenterServices.Add(serviceCenterService);
            await _context.SaveChangesAsync();

            var resultDto = new ServiceCenterServiceDTO
            {
                ServiceCenterServiceId = serviceCenterService.ServiceCenterServiceId,
                Station_id = serviceCenterService.Station_id,
                ServiceId = serviceCenterService.ServiceId,
                PackageId = serviceCenterService.PackageId,
                CustomPrice = serviceCenterService.CustomPrice,
                ServiceCenterBasePrice = serviceCenterService.BasePrice,
                ServiceCenterLoyaltyPoints = serviceCenterService.LoyaltyPoints,
                IsAvailable = serviceCenterService.IsAvailable,
                Notes = serviceCenterService.Notes,
                ServiceName = service.ServiceName,
                ServiceDescription = service.Description,
                ServiceBasePrice = service.BasePrice,
                Category = service.Category,
                StationName = serviceCenter.Station_name,
                PackageName = package != null ? package.PackageName : null,
                PackagePercentage = package != null ? package.Percentage : null,
                PackageDescription = package != null ? package.Description : null
            };

            return CreatedAtAction(nameof(GetServiceCenterService),
                new { id = serviceCenterService.ServiceCenterServiceId }, resultDto);
        }

        // PUT: api/ServiceCenterServices/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> UpdateServiceCenterService(int id, UpdateServiceCenterServiceDTO updateDto)
        {
            var serviceCenterService = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.Package)
                .FirstOrDefaultAsync(scs => scs.ServiceCenterServiceId == id);

            if (serviceCenterService == null)
            {
                return NotFound();
            }

            // Validate package if provided
            Package? package = serviceCenterService.Package;
            if (updateDto.PackageId.HasValue)
            {
                package = await _context.Packages.FindAsync(updateDto.PackageId.Value);
                if (package == null)
                {
                    return BadRequest("Invalid package ID");
                }
                if (!package.IsActive)
                {
                    return BadRequest("Selected package is not active");
                }
                serviceCenterService.PackageId = updateDto.PackageId.Value;
            }

            // Update base price if provided
            if (updateDto.ServiceCenterBasePrice.HasValue)
            {
                serviceCenterService.BasePrice = updateDto.ServiceCenterBasePrice.Value;
            }

            if (updateDto.CustomPrice.HasValue)
                serviceCenterService.CustomPrice = updateDto.CustomPrice;

            if (updateDto.IsAvailable.HasValue)
                serviceCenterService.IsAvailable = updateDto.IsAvailable.Value;

            if (updateDto.Notes != null)
                serviceCenterService.Notes = updateDto.Notes;

            // Recalculate loyalty points if base price or package changed
            if (updateDto.ServiceCenterBasePrice.HasValue || updateDto.PackageId.HasValue)
            {
                decimal basePrice = serviceCenterService.BasePrice ?? serviceCenterService.CustomPrice ?? serviceCenterService.Service.BasePrice ?? 0;
                serviceCenterService.LoyaltyPoints = _loyaltyPointsService.CalculateLoyaltyPoints(basePrice, package?.Percentage);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCenterServiceExists(id))
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

        // DELETE: api/ServiceCenterServices/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> DeleteServiceCenterService(int id)
        {
            var serviceCenterService = await _context.ServiceCenterServices.FindAsync(id);
            if (serviceCenterService == null)
            {
                return NotFound();
            }

            _context.ServiceCenterServices.Remove(serviceCenterService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/ServiceCenterServices/{id}/toggle-availability
        [HttpPatch("{id}/toggle-availability")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> ToggleServiceAvailability(int id)
        {
            var serviceCenterService = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package)
                .FirstOrDefaultAsync(scs => scs.ServiceCenterServiceId == id);

            if (serviceCenterService == null)
            {
                return NotFound("Service center service not found");
            }

            // Toggle the availability
            serviceCenterService.IsAvailable = !serviceCenterService.IsAvailable;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCenterServiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Return the updated service center service
            var resultDto = new ServiceCenterServiceDTO
            {
                ServiceCenterServiceId = serviceCenterService.ServiceCenterServiceId,
                Station_id = serviceCenterService.Station_id,
                ServiceId = serviceCenterService.ServiceId,
                PackageId = serviceCenterService.PackageId,
                CustomPrice = serviceCenterService.CustomPrice,
                ServiceCenterBasePrice = serviceCenterService.BasePrice,
                ServiceCenterLoyaltyPoints = serviceCenterService.LoyaltyPoints,
                IsAvailable = serviceCenterService.IsAvailable,
                Notes = serviceCenterService.Notes,
                ServiceName = serviceCenterService.Service.ServiceName,
                ServiceDescription = serviceCenterService.Service.Description,
                ServiceBasePrice = serviceCenterService.Service.BasePrice,
                Category = serviceCenterService.Service.Category,
                StationName = serviceCenterService.ServiceCenter.Station_name,
                PackageName = serviceCenterService.Package != null ? serviceCenterService.Package.PackageName : null,
                PackagePercentage = serviceCenterService.Package != null ? serviceCenterService.Package.Percentage : null,
                PackageDescription = serviceCenterService.Package != null ? serviceCenterService.Package.Description : null
            };

            return Ok(resultDto);
        }

        // POST: api/ServiceCenterServices/update-availability-from-closure
        [HttpPost("update-availability-from-closure")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> UpdateAvailabilityFromClosure([FromBody] UpdateAvailabilityFromClosureDTO dto)
        {
            // Get all services for the service center
            var serviceCenterServices = await _context.ServiceCenterServices
                .Where(scs => scs.Station_id == dto.ServiceCenterId)
                .ToListAsync();

            if (!serviceCenterServices.Any())
            {
                return NotFound("No services found for this service center");
            }

            // Check if the service center is closed on the specified date
            bool isClosed = await _context.ClosureSchedules
                .AnyAsync(cs => cs.ServiceCenterId == dto.ServiceCenterId &&
                               cs.ClosureDate.Date == dto.ClosureDate.Date);

            // Update availability based on closure status
            foreach (var service in serviceCenterServices)
            {
                // If service center is closed, make all services unavailable
                // If service center is open, restore availability to default (true)
                service.IsAvailable = !isClosed;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating service availability: {ex.Message}");
            }

            return Ok(new
            {
                message = $"Updated availability for {serviceCenterServices.Count} services",
                isClosed = isClosed,
                serviceCenterId = dto.ServiceCenterId,
                closureDate = dto.ClosureDate
            });
        }

        // PUT: api/ServiceCenters/{stationId}/Services/{serviceId}
        [HttpPut("ServiceCenters/{stationId}/Services/{serviceId}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> UpdateServiceAvailabilityByStationAndService(int stationId, int serviceId, [FromBody] UpdateServiceAvailabilityRequest request)
        {
            // Find the service center service by station ID and service ID
            var serviceCenterService = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package)
                .FirstOrDefaultAsync(scs => scs.Station_id == stationId && scs.ServiceId == serviceId);

            if (serviceCenterService == null)
            {
                return NotFound($"Service with ID {serviceId} not found for service center {stationId}");
            }

            // Update only the availability if provided
            if (request.IsAvailable.HasValue)
            {
                serviceCenterService.IsAvailable = request.IsAvailable.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceCenterServiceExists(serviceCenterService.ServiceCenterServiceId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Return the updated service center service
            var resultDto = new ServiceCenterServiceDTO
            {
                ServiceCenterServiceId = serviceCenterService.ServiceCenterServiceId,
                Station_id = serviceCenterService.Station_id,
                ServiceId = serviceCenterService.ServiceId,
                PackageId = serviceCenterService.PackageId,
                CustomPrice = serviceCenterService.CustomPrice,
                ServiceCenterBasePrice = serviceCenterService.BasePrice,
                ServiceCenterLoyaltyPoints = serviceCenterService.LoyaltyPoints,
                IsAvailable = serviceCenterService.IsAvailable,
                Notes = serviceCenterService.Notes,
                ServiceName = serviceCenterService.Service.ServiceName,
                ServiceDescription = serviceCenterService.Service.Description,
                ServiceBasePrice = serviceCenterService.Service.BasePrice,
                Category = serviceCenterService.Service.Category,
                StationName = serviceCenterService.ServiceCenter.Station_name,
                PackageName = serviceCenterService.Package != null ? serviceCenterService.Package.PackageName : null,
                PackagePercentage = serviceCenterService.Package != null ? serviceCenterService.Package.Percentage : null,
                PackageDescription = serviceCenterService.Package != null ? serviceCenterService.Package.Description : null
            };

            return Ok(resultDto);
        }

        private bool ServiceCenterServiceExists(int id)
        {
            return _context.ServiceCenterServices.Any(e => e.ServiceCenterServiceId == id);
        }
    }
}
