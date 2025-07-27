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
            return Ok(new
            {
                token,
                userId = user.UserId,
                userRole = user.UserRole.UserRoleName,
                userRoleId = user.UserRoleId // Explicitly include UserRoleId in response
            });
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


        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterDto dto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already registered.");

            // Validate that the UserRoleId exists
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(r => r.UserRoleId == dto.UserRoleId);

            if (userRole == null)
                return BadRequest($"Invalid UserRoleId: {dto.UserRoleId}. Role does not exist.");

            // Create new user with the specified role
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserRoleId = dto.UserRoleId // Use UserRoleId from DTO
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log the user creation with role information
            Console.WriteLine($"User created: {user.Email} with UserRoleId: {user.UserRoleId} ({userRole.UserRoleName})");

            return Ok(new
            {
                message = "User registered successfully",
                userId = user.UserId,
                email = user.Email,
                userRoleId = user.UserRoleId,
                userRoleName = userRole.UserRoleName
            });
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

        [HttpGet("user-roles")]
        public async Task<IActionResult> GetUserRoles()
        {
            var roles = await _context.UserRoles
                .OrderBy(r => r.UserRoleId)
                .Select(r => new
                {
                    userRoleId = r.UserRoleId,
                    userRoleName = r.UserRoleName
                })
                .ToListAsync();

            return Ok(roles);
        }

        [HttpGet("user-info/{userId}")]
        public async Task<IActionResult> GetUserInfo(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                userId = user.UserId,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                userRoleId = user.UserRoleId,
                userRoleName = user.UserRole.UserRoleName
            });
        }

        [HttpPut("update-user-role/{userId}")]
        [Authorize(Roles = "SuperAdmin,Admin")] // Only SuperAdmin and Admin can change roles
        public async Task<IActionResult> UpdateUserRole(int userId, [FromBody] UpdateUserRoleDto dto)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound("User not found.");

            // Validate that the new UserRoleId exists
            var newUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(r => r.UserRoleId == dto.UserRoleId);

            if (newUserRole == null)
                return BadRequest($"Invalid UserRoleId: {dto.UserRoleId}. Role does not exist.");

            var oldRoleName = user.UserRole.UserRoleName;
            var oldRoleId = user.UserRoleId;

            // Update the user's role
            user.UserRoleId = dto.UserRoleId;
            await _context.SaveChangesAsync();

            // Log the role change
            Console.WriteLine($"User role updated: {user.Email} changed from UserRoleId {oldRoleId} ({oldRoleName}) to UserRoleId {dto.UserRoleId} ({newUserRole.UserRoleName})");

            return Ok(new
            {
                message = "User role updated successfully",
                userId = user.UserId,
                email = user.Email,
                previousRole = new { userRoleId = oldRoleId, userRoleName = oldRoleName },
                newRole = new { userRoleId = dto.UserRoleId, userRoleName = newUserRole.UserRoleName }
            });
        }

        [HttpPost("debug-token")]
        public IActionResult DebugToken([FromBody] string token)
        {
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);

                var claims = jsonToken.Claims.Select(c => new
                {
                    Type = c.Type,
                    Value = c.Value
                }).ToList();

                return Ok(new
                {
                    claims = claims,
                    userRoleId = jsonToken.Claims.FirstOrDefault(c => c.Type == "UserRoleId")?.Value,
                    userId = jsonToken.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value,
                    role = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid token: {ex.Message}");
            }
        }

        [HttpGet("debug-users")]
        public async Task<IActionResult> DebugUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.UserRole)
                    .Select(u => new
                    {
                        userId = u.UserId,
                        firstName = u.FirstName,
                        lastName = u.LastName,
                        email = u.Email,
                        userRoleId = u.UserRoleId,
                        userRoleName = u.UserRole != null ? u.UserRole.UserRoleName : "NULL",
                        hasPassword = !string.IsNullOrEmpty(u.Password),
                        passwordLength = u.Password != null ? u.Password.Length : 0
                    })
                    .ToListAsync();

                var userRoles = await _context.UserRoles.ToListAsync();

                return Ok(new
                {
                    totalUsers = users.Count,
                    totalRoles = userRoles.Count,
                    users = users,
                    roles = userRoles.Select(r => new { r.UserRoleId, r.UserRoleName })
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Debug error: {ex.Message}");
            }
        }

        [HttpPost("test-password")]
        public async Task<IActionResult> TestPassword([FromBody] TestPasswordDto dto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRole)
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (user == null)
                {
                    return Ok(new
                    {
                        found = false,
                        message = "User not found with this email"
                    });
                }

                var passwordMatches = BCrypt.Net.BCrypt.Verify(dto.Password, user.Password);

                return Ok(new
                {
                    found = true,
                    passwordMatches = passwordMatches,
                    user = new
                    {
                        userId = user.UserId,
                        email = user.Email,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        userRoleId = user.UserRoleId,
                        userRoleName = user.UserRole?.UserRoleName,
                        hasPassword = !string.IsNullOrEmpty(user.Password),
                        passwordHash = user.Password?.Substring(0, Math.Min(20, user.Password.Length)) + "..."
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Test error: {ex.Message}");
            }
        }

        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            try
            {
                // Verify Google ID token
                var payload = await _tokenService.VerifyGoogleToken(dto.IdToken);

                // Check if customer exists
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == payload.Email);

                if (customer == null)
                {
                    // Create new customer
                    customer = new Customer
                    {
                        FirstName = payload.GivenName ?? "Unknown",
                        LastName = payload.FamilyName ?? "Unknown",
                        Email = payload.Email,
                        PhoneNumber = "Not provided", // Google doesn't provide phone
                        Address = "Not provided",
                        NIC = "Not provided",
                        IsEmailVerified = true, // Google email is verified by default
                        Password = null // No password for Google users
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }

                // Generate JWT token
                var token = _tokenService.CreateTokenForCustomer(customer);

                return Ok(new { token, customerId = customer.CustomerId });
            }
            catch (Exception ex)
            {
                return BadRequest($"Google login failed: {ex.Message}");
            }
        }
    }
}
