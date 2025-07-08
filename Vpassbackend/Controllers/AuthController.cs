using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Vpassbackend.DTOs;
using Microsoft.AspNetCore.Authorization;

using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _tokenService;

        public AuthController(ApplicationDbContext context, AuthService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = await _context.Users.Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var token = _tokenService.CreateToken(user, user.UserRole.UserRoleName);
            return Ok(new { token });
        }
        [HttpPost("login-customer")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginCustomer(CustomerLoginDto dto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == dto.Email);

            if (customer == null || !BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password))
                return Unauthorized("Invalid credentials");

            // Customers don’t have roles — so your TokenService needs an overload for them
            var token = _tokenService.CreateTokenForCustomer(customer);
            return Ok(new { token });
        }


        [HttpPost("register-customer")]
        public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto dto)
        {
            if (_context.Customers.Any(c => c.Email == dto.Email))
                return BadRequest("Email already registered.");

            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                NIC = dto.NIC,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok("Customer registered.");
        }
    }
}
