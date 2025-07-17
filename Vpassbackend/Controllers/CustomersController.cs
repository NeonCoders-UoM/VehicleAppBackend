using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Customers
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin,Cashier")]
        [HttpGet]
        public IActionResult GetAllCustomers()
        {
            var customers = _context.Customers
                .Select(c => new
                {
                    c.CustomerId,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.PhoneNumber,
                    c.Address,
                    c.LoyaltyPoints
                }).ToList();

            return Ok(customers);
        }

        // GET: api/Customers/5
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin,Cashier")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _context.Customers
                .Where(c => c.CustomerId == id)
                .Select(c => new
                {
                    c.CustomerId,
                    c.FirstName,
                    c.LastName,
                    c.Email,
                    c.PhoneNumber,
                    c.Address,
                    c.NIC,
                    c.LoyaltyPoints
                })
                .FirstOrDefaultAsync();

            if (customer == null)
            {
                return NotFound("Customer not found.");
            }

            return Ok(customer);
        }

        // PUT: api/Customers/5
        // POST: api/Customers/{customerId}/vehicles

        [HttpPost("{customerId}/vehicles")]
        public async Task<IActionResult> RegisterVehicle(int customerId, VehicleRegistrationDto dto)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound("Customer not found.");

            // Check if vehicle with this registration number already exists
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.RegistrationNumber == dto.RegistrationNumber);

            if (existingVehicle != null)
            {
                return BadRequest("A vehicle with this registration number already exists.");
            }

            var vehicle = new Vehicle
            {
                RegistrationNumber = dto.RegistrationNumber,
                CustomerId = customerId,
                Brand = dto.Brand,
                Model = dto.Model,
                ChassisNumber = dto.ChassisNumber,
                Mileage = dto.Mileage,
                Fuel = dto.Fuel,
                Year = dto.Year,
                Customer = customer
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetVehicleById),
                new { customerId, vehicleId = vehicle.VehicleId },
                vehicle
            );
        }

        [Authorize]// GET: api/Customers/{customerId}/vehicles
        [HttpGet("{customerId}/vehicles")]
        public async Task<IActionResult> GetCustomerVehicles(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound("Customer not found.");

            var vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == customerId)
                .Select(v => new
                {
                    v.VehicleId,
                    v.RegistrationNumber,
                    v.Brand,
                    v.Model,
                    v.ChassisNumber,
                    v.Mileage,
                    v.Fuel,
                    v.Year
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // GET: api/Customers/{customerId}/vehicles/{vehicleId}
        [Authorize]
        [HttpGet("{customerId}/vehicles/{vehicleId}")]
        public async Task<IActionResult> GetVehicleById(int customerId, int vehicleId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound("Customer not found.");

            var vehicle = await _context.Vehicles
                .Where(v => v.VehicleId == vehicleId && v.CustomerId == customerId)
                .Select(v => new
                {
                    v.VehicleId,
                    v.RegistrationNumber,
                    v.Brand,
                    v.Model,
                    v.ChassisNumber,
                    v.Mileage,
                    v.Fuel,
                    v.Year
                })
                .FirstOrDefaultAsync();

            if (vehicle == null)
            {
                return NotFound("Vehicle not found or doesn't belong to this customer.");
            }

            return Ok(vehicle);
        }

        // PUT: api/Customers/{customerId}/vehicles/{vehicleId}
        [Authorize]
        [HttpPut("{customerId}/vehicles/{vehicleId}")]
        public async Task<IActionResult> UpdateVehicle(int customerId, int vehicleId, VehicleRegistrationDto dto)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound("Customer not found.");

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId && v.CustomerId == customerId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found or doesn't belong to this customer.");
            }

            // Check if another vehicle with this registration number already exists
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.RegistrationNumber == dto.RegistrationNumber && v.VehicleId != vehicleId);

            if (existingVehicle != null)
            {
                return BadRequest("Another vehicle with this registration number already exists.");
            }

            vehicle.RegistrationNumber = dto.RegistrationNumber;
            vehicle.Brand = dto.Brand;
            vehicle.Model = dto.Model;
            vehicle.ChassisNumber = dto.ChassisNumber;
            vehicle.Mileage = dto.Mileage;
            vehicle.Fuel = dto.Fuel;
            vehicle.Year = dto.Year;

            await _context.SaveChangesAsync();
            return Ok("Vehicle updated successfully.");
        }

        // DELETE: api/Customers/{customerId}/vehicles/{vehicleId}
        [Authorize]
        [HttpDelete("{customerId}/vehicles/{vehicleId}")]
        public async Task<IActionResult> DeleteVehicle(int customerId, int vehicleId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null) return NotFound("Customer not found.");

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId && v.CustomerId == customerId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found or doesn't belong to this customer.");
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();
            return Ok("Vehicle deleted successfully.");
        }
    }
}
