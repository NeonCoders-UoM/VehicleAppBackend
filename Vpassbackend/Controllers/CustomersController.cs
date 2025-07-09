using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;
using Vpassbackend.Services;
using System.Security.Claims;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;

        public CustomersController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // ✅ Correct method: GET
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
                    c.LoyaltyPoints
                }).ToList();

            return Ok(customers);
        }

        // ✅ Correct method: PUT
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.Address = dto.Address;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.NIC = dto.NIC;


            await _context.SaveChangesAsync();
            return Ok("Profile updated.");
        }

        // Register a new customer
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto dto)
        {
            // Check if email already exists
            if (await _context.Customers.AnyAsync(c => c.Email == dto.Email))
            {
                return BadRequest(new { message = "Email is already registered" });
            }

            // Create new customer with hashed password
            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                NIC = dto.NIC,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                LoyaltyPoints = 0
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Generate token for immediate login
            var token = _authService.CreateTokenForCustomer(customer);

            return Ok(new
            {
                message = "Registration successful",
                customerId = customer.CustomerId,
                token
            });
        }

        // Get current customer profile (requires authentication)
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetCustomerProfile()
        {
            // Get customer ID from JWT token claims
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId) || !int.TryParse(customerId, out int id))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            return Ok(new
            {
                customer.CustomerId,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.Address,
                customer.PhoneNumber,
                customer.NIC,
                customer.LoyaltyPoints
            });
        }

        // Register a vehicle for the current customer
        [Authorize]
        [HttpPost("vehicles")]
        public async Task<IActionResult> AddVehicle(VehicleRegistrationDTO dto)
        {
            // Get customer ID from JWT token claims
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId) || !int.TryParse(customerId, out int id))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            // Check if registration number already exists
            if (await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == dto.RegistrationNumber))
            {
                return BadRequest(new { message = "Vehicle with this registration number already exists" });
            }

            var vehicle = new Vehicle
            {
                RegistrationNumber = dto.RegistrationNumber,
                CustomerId = id,
                Brand = dto.Brand,
                Model = dto.Model,
                ChassisNumber = dto.ChassisNumber,
                Mileage = dto.Mileage,
                Fuel = dto.Fuel,
                Year = dto.Year,
                Customer = await _context.Customers.FindAsync(id)
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCustomerVehicles),
                new { },
                new
                {
                    vehicle.VehicleId,
                    vehicle.RegistrationNumber,
                    vehicle.Brand,
                    vehicle.Model,
                    vehicle.Year
                }
            );
        }

        // Get all vehicles for the current customer
        [Authorize]
        [HttpGet("vehicles")]
        public async Task<IActionResult> GetCustomerVehicles()
        {
            // Get customer ID from JWT token claims
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId) || !int.TryParse(customerId, out int id))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == id)
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

        // Get a specific vehicle by ID
        [Authorize]
        [HttpGet("vehicles/{vehicleId}")]
        public async Task<IActionResult> GetVehicleById(int vehicleId)
        {
            // Get customer ID from JWT token claims
            var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(customerId) || !int.TryParse(customerId, out int id))
            {
                return Unauthorized(new { message = "Invalid authentication token" });
            }

            var vehicle = await _context.Vehicles
                .Where(v => v.CustomerId == id && v.VehicleId == vehicleId)
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
                return NotFound(new { message = "Vehicle not found or you don't have access" });
            }

            return Ok(vehicle);
        }

        // Register a new customer with a vehicle in one step
        [AllowAnonymous]
        [HttpPost("register-with-vehicle")]
        public async Task<IActionResult> RegisterCustomerWithVehicle(CustomerWithVehicleRegistrationDTO dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Check if email already exists
                if (await _context.Customers.AnyAsync(c => c.Email == dto.Email))
                {
                    return BadRequest(new { message = "Email is already registered" });
                }

                // Check if registration number already exists
                if (await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == dto.RegistrationNumber))
                {
                    return BadRequest(new { message = "Vehicle with this registration number already exists" });
                }

                // Create new customer with hashed password
                var customer = new Customer
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Address = dto.Address,
                    PhoneNumber = dto.PhoneNumber,
                    NIC = dto.NIC,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    LoyaltyPoints = 0
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Create vehicle for the customer
                var vehicle = new Vehicle
                {
                    RegistrationNumber = dto.RegistrationNumber,
                    CustomerId = customer.CustomerId,
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

                // Commit the transaction
                await transaction.CommitAsync();

                // Generate token for immediate login
                var token = _authService.CreateTokenForCustomer(customer);

                return Ok(new
                {
                    message = "Registration successful",
                    customerId = customer.CustomerId,
                    vehicleId = vehicle.VehicleId,
                    token
                });
            }
            catch (Exception ex)
            {
                // Roll back the transaction in case of an error
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred during registration", error = ex.Message });
            }
        }
    }
}
