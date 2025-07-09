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
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Vehicles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            return await _context.Vehicles.ToListAsync();
        }

        // GET: api/Vehicles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.ServiceHistory)
                .FirstOrDefaultAsync(v => v.VehicleId == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            return vehicle;
        }

        // POST: api/Vehicles/5/ServiceHistory
        [HttpPost("{vehicleId}/ServiceHistory")]
        public async Task<ActionResult<VehicleServiceHistory>> AddServiceHistory(int vehicleId, AddServiceHistoryDTO dto)
        {
            if (vehicleId != dto.VehicleId)
            {
                return BadRequest("Vehicle ID mismatch");
            }

            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            var serviceCenter = await _context.ServiceCenters.FindAsync(dto.ServiceCenterId);
            if (serviceCenter == null)
            {
                return NotFound("Service center not found");
            }

            var serviceHistory = new VehicleServiceHistory
            {
                VehicleId = dto.VehicleId,
                ServiceType = dto.ServiceType,
                Description = dto.Description ?? "",
                Cost = dto.Cost,
                ServiceCenterId = dto.ServiceCenterId,
                ServicedByUserId = dto.ServicedByUserId,
                ServiceDate = dto.ServiceDate ?? DateTime.Now,
                Vehicle = vehicle,
                ServiceCenter = serviceCenter
            };

            _context.VehicleServiceHistory.Add(serviceHistory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetServiceHistory),
                new { vehicleId = vehicle.VehicleId, serviceHistoryId = serviceHistory.ServiceHistoryId },
                serviceHistory
            );
        }

        // GET: api/Vehicles/5/ServiceHistory
        [HttpGet("{vehicleId}/ServiceHistory")]
        public async Task<ActionResult<IEnumerable<VehicleServiceHistory>>> GetVehicleServiceHistory(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            return await _context.VehicleServiceHistory
                .Where(sh => sh.VehicleId == vehicleId)
                .Include(sh => sh.ServiceCenter)
                .Include(sh => sh.ServicedByUser)
                .ToListAsync();
        }

        // GET: api/Vehicles/5/ServiceHistory/1
        [HttpGet("{vehicleId}/ServiceHistory/{serviceHistoryId}")]
        public async Task<ActionResult<VehicleServiceHistory>> GetServiceHistory(int vehicleId, int serviceHistoryId)
        {
            var serviceHistory = await _context.VehicleServiceHistory
                .Include(sh => sh.ServiceCenter)
                .Include(sh => sh.ServicedByUser)
                .FirstOrDefaultAsync(sh => sh.VehicleId == vehicleId && sh.ServiceHistoryId == serviceHistoryId);

            if (serviceHistory == null)
            {
                return NotFound("Service history not found");
            }

            return serviceHistory;
        }

        // PUT: api/Vehicles/5/ServiceHistory/1
        [HttpPut("{vehicleId}/ServiceHistory/{serviceHistoryId}")]
        public async Task<IActionResult> UpdateServiceHistory(int vehicleId, int serviceHistoryId, VehicleServiceHistory serviceHistory)
        {
            if (serviceHistoryId != serviceHistory.ServiceHistoryId || vehicleId != serviceHistory.VehicleId)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(serviceHistory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceHistoryExists(serviceHistoryId))
                {
                    return NotFound("Service history not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool ServiceHistoryExists(int id)
        {
            return _context.VehicleServiceHistory.Any(e => e.ServiceHistoryId == id);
        }
    }
}
