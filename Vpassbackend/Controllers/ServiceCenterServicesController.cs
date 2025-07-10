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
    public class ServiceCenterServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceCenterServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceCenterServices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCenterServiceDTO>>> GetServiceCenterServices()
        {
            var serviceCenterServices = await _context.ServiceCenterServices
                .Include(scs => scs.Service)
                .Include(scs => scs.ServiceCenter)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    CustomPrice = scs.CustomPrice,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    BasePrice = scs.Service.BasePrice,
                    LoyaltyPoints = scs.Service.LoyaltyPoints,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name
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
                .Where(scs => scs.ServiceCenterServiceId == id)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    CustomPrice = scs.CustomPrice,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    BasePrice = scs.Service.BasePrice,
                    LoyaltyPoints = scs.Service.LoyaltyPoints,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name
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
                .Where(scs => scs.Station_id == stationId)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    CustomPrice = scs.CustomPrice,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    BasePrice = scs.Service.BasePrice,
                    LoyaltyPoints = scs.Service.LoyaltyPoints,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name
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
                .Where(scs => scs.ServiceId == serviceId)
                .Select(scs => new ServiceCenterServiceDTO
                {
                    ServiceCenterServiceId = scs.ServiceCenterServiceId,
                    Station_id = scs.Station_id,
                    ServiceId = scs.ServiceId,
                    CustomPrice = scs.CustomPrice,
                    IsAvailable = scs.IsAvailable,
                    Notes = scs.Notes,
                    ServiceName = scs.Service.ServiceName,
                    ServiceDescription = scs.Service.Description,
                    BasePrice = scs.Service.BasePrice,
                    LoyaltyPoints = scs.Service.LoyaltyPoints,
                    Category = scs.Service.Category,
                    StationName = scs.ServiceCenter.Station_name
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

            // Check if the relation already exists
            bool alreadyExists = await _context.ServiceCenterServices
                .AnyAsync(scs => scs.ServiceId == createDto.ServiceId && scs.Station_id == createDto.Station_id);

            if (alreadyExists)
            {
                return BadRequest("This service is already offered by this service center");
            }

            var serviceCenterService = new ServiceCenterService
            {
                Station_id = createDto.Station_id,
                ServiceId = createDto.ServiceId,
                CustomPrice = createDto.CustomPrice,
                IsAvailable = createDto.IsAvailable,
                Notes = createDto.Notes,
                ServiceCenter = serviceCenter,
                Service = service
            };

            _context.ServiceCenterServices.Add(serviceCenterService);
            await _context.SaveChangesAsync();

            var resultDto = new ServiceCenterServiceDTO
            {
                ServiceCenterServiceId = serviceCenterService.ServiceCenterServiceId,
                Station_id = serviceCenterService.Station_id,
                ServiceId = serviceCenterService.ServiceId,
                CustomPrice = serviceCenterService.CustomPrice,
                IsAvailable = serviceCenterService.IsAvailable,
                Notes = serviceCenterService.Notes,
                ServiceName = service.ServiceName,
                ServiceDescription = service.Description,
                BasePrice = service.BasePrice,
                LoyaltyPoints = service.LoyaltyPoints,
                Category = service.Category,
                StationName = serviceCenter.Station_name
            };

            return CreatedAtAction(nameof(GetServiceCenterService), 
                new { id = serviceCenterService.ServiceCenterServiceId }, resultDto);
        }

        // PUT: api/ServiceCenterServices/5
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin")]
        public async Task<IActionResult> UpdateServiceCenterService(int id, UpdateServiceCenterServiceDTO updateDto)
        {
            var serviceCenterService = await _context.ServiceCenterServices.FindAsync(id);
            if (serviceCenterService == null)
            {
                return NotFound();
            }

            if (updateDto.CustomPrice.HasValue)
                serviceCenterService.CustomPrice = updateDto.CustomPrice;

            if (updateDto.IsAvailable.HasValue)
                serviceCenterService.IsAvailable = updateDto.IsAvailable.Value;

            if (updateDto.Notes != null)
                serviceCenterService.Notes = updateDto.Notes;

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

        private bool ServiceCenterServiceExists(int id)
        {
            return _context.ServiceCenterServices.Any(e => e.ServiceCenterServiceId == id);
        }
    }
}
