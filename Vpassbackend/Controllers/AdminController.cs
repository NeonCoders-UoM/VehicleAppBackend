using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Vpassbackend.DTOs;

namespace Vpassbackend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("user-register")]
        public async Task<IActionResult> CreateUser(UserRegisterDto dto)
        {
            if (_context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("Email already exists.");

            // Validate ServiceCenterAdmin, Cashier, and DataOperator requirements
            if (dto.UserRoleId == 3 || dto.UserRoleId == 4 || dto.UserRoleId == 5) // ServiceCenterAdmin, Cashier, DataOperator
            {
                if (!dto.Station_id.HasValue)
                    return BadRequest("ServiceCenterAdmin, Cashier, and DataOperator must be assigned to a service center (Station_id is required).");

                // Validate that the service center exists
                var serviceCenter = await _context.ServiceCenters.FindAsync(dto.Station_id.Value);
                if (serviceCenter == null)
                    return BadRequest($"Service center with Station_id {dto.Station_id.Value} does not exist.");
            }

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserRoleId = dto.UserRoleId,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Station_id = dto.Station_id // Assign to specific service center if provided
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User created.");
        }
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin,Cashier")]
        [HttpGet("all-users")]
        public IActionResult GetAllUsers()
        {
            var users = _context.Users
                .Include(u => u.UserRole)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    Role = u.UserRole.UserRoleName
                }).ToList();

            return Ok(users);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.UserRoleId = dto.UserRoleId;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return Ok("User updated successfully");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User deleted.");
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    userId = u.UserId,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    Role = u.UserRole.UserRoleName// Adjust if you have a navigation property
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

    }
}
