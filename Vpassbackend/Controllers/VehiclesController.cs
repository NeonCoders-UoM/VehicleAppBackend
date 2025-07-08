using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehiclesByCustomerId(int customerId)
        {
            var vehicles = await _vehicleService.GetVehiclesByCustomerIdAsync(customerId);
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<ActionResult<VehicleDto>> CreateVehicle(VehicleCreateDto vehicleCreateDto)
        {
            var vehicle = await _vehicleService.CreateVehicleAsync(vehicleCreateDto);
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.VehicleId }, vehicle);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<VehicleDto>> UpdateVehicle(int id, VehicleUpdateDto vehicleUpdateDto)
        {
            var vehicle = await _vehicleService.UpdateVehicleAsync(id, vehicleUpdateDto);
            if (vehicle == null)
            {
                return NotFound();
            }

            return Ok(vehicle);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteVehicle(int id)
        {
            var result = await _vehicleService.DeleteVehicleAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
