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
        private readonly IEmailService _emailService;

        public AuthController(ApplicationDbContext context, AuthService tokenService, IEmailService emailService)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
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

            if (customer == null)
                return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password))
                return Unauthorized("Invalid credentials");

            if (!customer.IsEmailVerified)
                return Unauthorized("Email not verified. Please verify your email first.");

            var token = _tokenService.CreateTokenForCustomer(customer);

            // ✅ Include the CustomerId in your response
            return Ok(new { token, customerId = customer.CustomerId });
        }


        [HttpPost("register-customer")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer(CustomerRegisterDto dto)
        {
            if (_context.Customers.Any(c => c.Email == dto.Email))
                return BadRequest("Email already registered.");

            // Generate random 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            var customer = new Customer
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                NIC = dto.NIC,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IsEmailVerified = false,
                OtpCode = otp,
                OtpExpiry = otpExpiry
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            // Send OTP email
            await _emailService.SendEmailAsync(
                customer.Email,
                "Your OTP Code",
                $"Your OTP code is {otp}. It expires in 10 minutes."
            );

            return Ok("Customer registered. Please check your email for the OTP to verify your account.");
        }


        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (customer == null)
                return BadRequest("Customer not found.");

            if (customer.IsEmailVerified)
                return BadRequest("Email already verified.");

            if (customer.OtpCode != dto.Otp)  // ✅ Correct variable
                return BadRequest("Invalid OTP.");

            if (customer.OtpExpiry < DateTime.UtcNow)
                return BadRequest("OTP expired.");

            customer.IsEmailVerified = true;
            customer.OtpCode = null;
            customer.OtpExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Email verified successfully!");
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] string email)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            if (customer == null)
                return BadRequest("Customer not found.");

            if (customer.IsEmailVerified)
                return BadRequest("Email already verified.");

            var otp = new Random().Next(100000, 999999).ToString();
            customer.OtpCode = otp;
            customer.OtpExpiry = DateTime.UtcNow.AddMinutes(10);

            await _context.SaveChangesAsync();

            await _emailService.SendEmailAsync(
                customer.Email,
                "Your new OTP",
                $"<p>Your new OTP is: <strong>{otp}</strong></p>"
            );

            return Ok("OTP resent successfully.");
        }

        [HttpPut("update-customer-details")]
        public async Task<IActionResult> UpdateCustomerDetails([FromBody] CustomerUpdateDto dto)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (customer == null) return NotFound("Customer not found.");

            customer.FirstName = dto.FirstName;
            customer.LastName = dto.LastName;
            customer.PhoneNumber = dto.PhoneNumber;
            customer.NIC = dto.NIC;
            customer.Address = dto.Address;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Customer details updated.", customerId = customer.CustomerId });
        }






    }
}
