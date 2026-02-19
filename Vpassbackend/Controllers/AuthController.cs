using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Vpassbackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
                .Include(u => u.ServiceCenter) // Include ServiceCenter for station_id info
                .FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized("Invalid credentials");

            var token = _tokenService.CreateToken(user, user.UserRole.UserRoleName);
            return Ok(new
            {
                token,
                userId = user.UserId,
                userRole = user.UserRole.UserRoleName,
                userRoleId = user.UserRoleId,
                station_id = user.Station_id, // Include station_id for service center admins
                serviceCenterName = user.ServiceCenter?.Station_name // Include service center name if available
            });
        }

        // GET: api/Auth/me - Get current user from token
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized("Invalid token");
                }

                var user = await _context.Users
                    .Include(u => u.UserRole)
                    .Include(u => u.ServiceCenter)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(new
                {
                    userId = user.UserId,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    userRole = user.UserRole.UserRoleName,
                    userRoleId = user.UserRoleId,
                    station_id = user.Station_id,
                    serviceCenterName = user.ServiceCenter?.Station_name
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error fetching user: {ex.Message}");
            }
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

        [HttpPost("google-login-customer")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLoginCustomer(GoogleLoginDto dto)
        {
            try
            {
                // Verify the Google ID token
                var payload = await Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

                var email = payload.Email;
                var googleId = payload.Subject;
                var firstName = payload.GivenName ?? "User";
                var lastName = payload.FamilyName ?? "";

                // Check if customer already exists
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (customer == null)
                {
                    // Create a new customer for Google sign-in
                    customer = new Customer
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        Password = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Random password
                        PhoneNumber = "",
                        Address = "",
                        NIC = "",
                        IsEmailVerified = true, // Google accounts are already verified
                        AuthProvider = "google",
                        GoogleId = googleId,
                        LoyaltyPoints = 0,
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Link Google to existing account if not already linked
                    if (string.IsNullOrEmpty(customer.GoogleId))
                    {
                        customer.GoogleId = googleId;
                        customer.AuthProvider = customer.AuthProvider == "local" ? "local,google" : customer.AuthProvider;
                        await _context.SaveChangesAsync();
                    }
                }

                var token = _tokenService.CreateTokenForCustomer(customer);
                return Ok(new { token, customerId = customer.CustomerId, isNewUser = customer.PhoneNumber == "" });
            }
            catch (Google.Apis.Auth.InvalidJwtException)
            {
                return Unauthorized("Invalid Google token");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Google authentication failed: {ex.Message}");
            }
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

            // Create new user with the specified role
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                UserRoleId = dto.UserRoleId, // Use UserRoleId from DTO
                Station_id = dto.Station_id // Assign to specific service center if provided
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Log the user creation with role information
            Console.WriteLine($"User created: {user.Email} with UserRoleId: {user.UserRoleId} ({userRole.UserRoleName})" + 
                (user.Station_id.HasValue ? $" assigned to Station_id: {user.Station_id}" : ""));

            return Ok(new
            {
                message = "User registered successfully",
                userId = user.UserId,
                email = user.Email,
                userRoleId = user.UserRoleId,
                userRoleName = userRole.UserRoleName,
                station_id = user.Station_id
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


        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            // Check for customer first
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (customer == null && user == null)
                return BadRequest("No account found with this email address.");

            // Generate random 6-digit OTP for password reset
            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            string firstName = "";
            string userType = "";

            if (customer != null)
            {
                customer.ForgotPasswordOtp = otp;
                customer.ForgotPasswordOtpExpiry = otpExpiry;
                firstName = customer.FirstName;
                userType = "Customer";
            }
            else if (user != null)
            {
                user.ForgotPasswordOtp = otp;
                user.ForgotPasswordOtpExpiry = otpExpiry;
                firstName = user.FirstName;
                userType = "Staff";
            }

            await _context.SaveChangesAsync();

            // Send password reset OTP email
            await _emailService.SendEmailAsync(
                dto.Email,
                "Password Reset Request",
                $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Password Reset Request</h2>
                    <p>Hello {firstName},</p>
                    <p>We received a request to reset your password for your {userType} account. Use the following OTP to reset your password:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #007bff; margin: 0; font-size: 32px;'>{otp}</h1>
                    </div>
                    <p><strong>This OTP will expire in 10 minutes.</strong></p>
                    <p>If you didn't request this password reset, please ignore this email.</p>
                    <p>Best regards,<br>Vehicle Service Team</p>
                </div>"
            );

            return Ok("Password reset OTP sent to your email address.");
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            // Check for customer first
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (customer == null && user == null)
                return BadRequest("No account found with this email address.");

            if (customer != null)
            {
                if (customer.ForgotPasswordOtp != dto.Otp)
                    return BadRequest("Invalid OTP.");

                if (customer.ForgotPasswordOtpExpiry < DateTime.UtcNow)
                    return BadRequest("OTP has expired.");

                // Update password
                customer.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                
                // Clear the forgot password OTP fields
                customer.ForgotPasswordOtp = null;
                customer.ForgotPasswordOtpExpiry = null;
            }
            else if (user != null)
            {
                if (user.ForgotPasswordOtp != dto.Otp)
                    return BadRequest("Invalid OTP.");

                if (user.ForgotPasswordOtpExpiry < DateTime.UtcNow)
                    return BadRequest("OTP has expired.");

                // Update password
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                
                // Clear the forgot password OTP fields
                user.ForgotPasswordOtp = null;
                user.ForgotPasswordOtpExpiry = null;
            }

            await _context.SaveChangesAsync();

            return Ok("Password reset successfully. You can now login with your new password.");
        }

        [HttpPost("resend-forgot-password-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendForgotPasswordOtp([FromBody] string email)
        {
            // Check for customer first
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (customer == null && user == null)
                return BadRequest("No account found with this email address.");

            // Generate new OTP
            var otp = new Random().Next(100000, 999999).ToString();
            var otpExpiry = DateTime.UtcNow.AddMinutes(10);

            string firstName = "";
            string userType = "";

            if (customer != null)
            {
                customer.ForgotPasswordOtp = otp;
                customer.ForgotPasswordOtpExpiry = otpExpiry;
                firstName = customer.FirstName;
                userType = "Customer";
            }
            else if (user != null)
            {
                user.ForgotPasswordOtp = otp;
                user.ForgotPasswordOtpExpiry = otpExpiry;
                firstName = user.FirstName;
                userType = "Staff";
            }

            await _context.SaveChangesAsync();

            // Send new password reset OTP email
            await _emailService.SendEmailAsync(
                email,
                "New Password Reset OTP",
                $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>New Password Reset OTP</h2>
                    <p>Hello {firstName},</p>
                    <p>Here's your new password reset OTP for your {userType} account:</p>
                    <div style='background-color: #f5f5f5; padding: 15px; text-align: center; margin: 20px 0;'>
                        <h1 style='color: #007bff; margin: 0; font-size: 32px;'>{otp}</h1>
                    </div>
                    <p><strong>This OTP will expire in 10 minutes.</strong></p>
                    <p>If you didn't request this password reset, please ignore this email.</p>
                    <p>Best regards,<br>Vehicle Service Team</p>
                </div>"
            );

            return Ok("New password reset OTP sent to your email address.");
        }

        [Authorize]
[HttpPost("change-password")]
public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
{
    // Find the customer
    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
    if (customer == null)
        return NotFound("Customer not found.");

    // Verify old password
    if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, customer.Password))
        return BadRequest("Old password is incorrect.");

    // Optionally: check new password strength here

    // Hash and set new password
    customer.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Password changed successfully." });
}
[Authorize]
[HttpPost("delete-account")]
public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
{
    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == dto.CustomerId);
    if (customer == null)
        return NotFound("Customer not found.");

    if (!BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password))
        return BadRequest("Password is incorrect.");

    _context.Customers.Remove(customer);
    await _context.SaveChangesAsync();

    return Ok(new { message = "Account deleted successfully." });
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
    }
}
