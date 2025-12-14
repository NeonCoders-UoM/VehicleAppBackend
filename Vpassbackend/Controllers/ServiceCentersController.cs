using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;
using Vpassbackend.Services;
using System;


namespace Vpassbackend.Controllers
{
    // [Authorize(Roles = "SuperAdmin")]
    [ApiController]
    [Route("api/[controller]")]

    public class ServiceCentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoyaltyPointsService _loyaltyPointsService;
        private readonly IGoogleMapsService _googleMapsService;

        public ServiceCentersController(ApplicationDbContext context, ILoyaltyPointsService loyaltyPointsService, IGoogleMapsService googleMapsService)
        {
            _context = context;
            _loyaltyPointsService = loyaltyPointsService;
            _googleMapsService = googleMapsService;
        }

        // GET: api/ServiceCenters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCenterDTO>>> GetServiceCenters()
        {
            // Get user role and ID from JWT token
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            IQueryable<ServiceCenter> query = _context.ServiceCenters;

            // If ServiceCenterAdmin, only show their assigned service center
            if (userRole == "ServiceCenterAdmin" && !string.IsNullOrEmpty(userIdClaim))
            {
                var userId = int.Parse(userIdClaim);
                var user = await _context.Users.FindAsync(userId);
                if (user?.Station_id != null)
                {
                    query = query.Where(sc => sc.Station_id == user.Station_id);
                }
            }

            var serviceCenters = await query
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
                    Station_status = sc.Station_status,
                    Latitude = sc.Latitude,
                    Longitude = sc.Longitude,
                    DefaultDailyAppointmentLimit = sc.DefaultDailyAppointmentLimit
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
                Station_status = serviceCenter.Station_status,
                Latitude = serviceCenter.Latitude,
                Longitude = serviceCenter.Longitude,
                DefaultDailyAppointmentLimit = serviceCenter.DefaultDailyAppointmentLimit
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
                    Station_status = sc.Station_status,
                    Latitude = sc.Latitude,
                    Longitude = sc.Longitude,
                    DefaultDailyAppointmentLimit = sc.DefaultDailyAppointmentLimit
                })
                .ToListAsync();

            return serviceCenters;
        }

        // POST: api/ServiceCenters
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ServiceCenterDTO>> CreateServiceCenter(CreateServiceCenterDTO createServiceCenterDTO)
        {
            // Get coordinates from address if not provided
            if ((!createServiceCenterDTO.Latitude.HasValue || !createServiceCenterDTO.Longitude.HasValue)
                && !string.IsNullOrEmpty(createServiceCenterDTO.Address))
            {
                var (lat, lng) = await _googleMapsService.GetCoordinatesFromAddressAsync(createServiceCenterDTO.Address);
                createServiceCenterDTO.Latitude = lat;
                createServiceCenterDTO.Longitude = lng;
            }

            var serviceCenter = new ServiceCenter
            {
                OwnerName = createServiceCenterDTO.OwnerName,
                VATNumber = createServiceCenterDTO.VATNumber,
                RegisterationNumber = createServiceCenterDTO.RegisterationNumber,
                Station_name = createServiceCenterDTO.Station_name,
                Email = createServiceCenterDTO.Email,
                Telephone = createServiceCenterDTO.Telephone,
                Address = createServiceCenterDTO.Address,
                Station_status = createServiceCenterDTO.Station_status,
                Latitude = createServiceCenterDTO.Latitude ?? 0,    // Use geocoded or default to 0
                Longitude = createServiceCenterDTO.Longitude ?? 0,  // Use geocoded or default to 0
                DefaultDailyAppointmentLimit = createServiceCenterDTO.DefaultDailyAppointmentLimit
            };

            _context.ServiceCenters.Add(serviceCenter);
            await _context.SaveChangesAsync();

            // Add default daily limits for the next 30 days using configurable limit
            var maxAppointments = createServiceCenterDTO.DefaultDailyAppointmentLimit;
            var startDate = DateOnly.FromDateTime(DateTime.Today);

            for (int i = 0; i < 30; i++)
            {
                var dailyLimit = new ServiceCenterDailyLimit
                {
                    Station_id = serviceCenter.Station_id,
                    Date = startDate.AddDays(i),
                    MaxAppointments = maxAppointments,
                    CurrentAppointments = 0
                };
                _context.ServiceCenterDailyLimits.Add(dailyLimit);
            }
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
                Station_status = serviceCenter.Station_status,
                Latitude = serviceCenter.Latitude,             // Added Latitude
                Longitude = serviceCenter.Longitude,           // Added Longitude
                DefaultDailyAppointmentLimit = serviceCenter.DefaultDailyAppointmentLimit
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

        // GET: api/ServiceCenters/{id}/Services
        [HttpGet("{id}/Services")]
        public async Task<ActionResult<IEnumerable<ServiceCenterServiceDTO>>> GetServiceCenterServices(int id)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return NotFound("Service center not found");
            }

            var serviceCenterServices = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Include(scs => scs.Package)
                .Where(scs => scs.Station_id == id)
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

        // POST: api/ServiceCenters/{id}/Services
        [HttpPost("{id}/Services")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<ActionResult<ServiceCenterServiceDTO>> AddServiceToServiceCenter(int id, CreateServiceCenterServiceDTO createDto)
        {
            var serviceCenter = await _context.ServiceCenters.FindAsync(id);
            if (serviceCenter == null)
            {
                return NotFound("Service center not found");
            }

            // Override the station_id to ensure it matches the URL parameter
            createDto.Station_id = id;

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
                .AnyAsync(scs => scs.ServiceId == createDto.ServiceId && scs.Station_id == id);

            if (alreadyExists)
            {
                return BadRequest("This service is already offered by this service center");
            }

            // Base price always comes from the system service registration
            decimal basePrice = service.BasePrice ?? 0;

            // Calculate loyalty points based on the base price (not custom price)
            int loyaltyPoints = _loyaltyPointsService.CalculateLoyaltyPoints(basePrice, package?.Percentage);

            var serviceCenterService = new ServiceCenterService
            {
                Station_id = id,
                ServiceId = createDto.ServiceId,
                PackageId = createDto.PackageId,
                CustomPrice = createDto.CustomPrice,
                BasePrice = basePrice, // Always from system service
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

            return CreatedAtAction("GetServiceCenterService",
                new { id = serviceCenterService.ServiceCenterServiceId, controller = "ServiceCenterServices" }, resultDto);
        }

        // DELETE: api/ServiceCenters/{stationId}/Services/{serviceId}
        [HttpDelete("{stationId}/Services/{serviceId}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> RemoveServiceFromServiceCenter(int stationId, int serviceId)
        {
            var serviceCenterService = await _context.ServiceCenterServices
                .FirstOrDefaultAsync(scs => scs.ServiceId == serviceId && scs.Station_id == stationId);

            if (serviceCenterService == null)
            {
                return NotFound("Service not found in this service center");
            }

            // Check if this service has any appointments
            bool hasAppointments = await _context.AppointmentServices.AnyAsync(a => a.ServiceId == serviceId);
            if (hasAppointments)
            {
                return BadRequest("Cannot remove service as it has associated appointments");
            }

            _context.ServiceCenterServices.Remove(serviceCenterService);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/ServiceCenters/{stationId}/Services/{serviceId}
        [HttpPut("{stationId}/Services/{serviceId}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> UpdateServiceAvailability(int stationId, int serviceId, [FromBody] UpdateServiceAvailabilityRequest request)
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

        // GET: api/servicecenters/nearby?lat={latitude}&lng={longitude}
        [HttpGet("nearby")]
        public async Task<ActionResult<IEnumerable<ServiceCenterSearchResultDTO>>> GetNearbyServiceCenters(
            double lat,
            double lng,
            [FromQuery] List<int> serviceIds,
            [FromQuery] DateTime appointmentDate)
        {
            var searchService = new ServiceCenterSearchService(_context);
            var results = await searchService.GetAvailableServiceCentersAsync(lat, lng, serviceIds, appointmentDate);
            return Ok(results);
        }


        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Earth's radius in km
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double deg) => deg * Math.PI / 180;

        private bool ServiceCenterExists(int id)
        {
            return _context.ServiceCenters.Any(e => e.Station_id == id);
        }

        private bool ServiceCenterServiceExists(int id)
        {
            return _context.ServiceCenterServices.Any(e => e.ServiceCenterServiceId == id);
        }
    }
}