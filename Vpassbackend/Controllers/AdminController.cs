using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.Models;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AdminController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

            // Store the plain password before hashing for email notification
            var plainPassword = dto.Password;

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

            // Send welcome email to Cashiers and Data Operators with their credentials
            if (dto.UserRoleId == 4 || dto.UserRoleId == 5) // Cashier or DataOperator
            {
                try
                {
                    var roleName = dto.UserRoleId == 4 ? "Cashier" : "Data Operator";
                    var serviceCenter = dto.Station_id.HasValue 
                        ? await _context.ServiceCenters.FindAsync(dto.Station_id.Value)
                        : null;
                    var serviceCenterName = serviceCenter?.Station_name ?? "Service Center";

                    var emailSubject = $"Welcome to {serviceCenterName} - Your Account Details";
                    var emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;'>
                                <div style='background-color: #ffffff; padding: 30px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
                                    <h2 style='color: #2563eb; margin-top: 0;'>Welcome to {serviceCenterName}!</h2>
                                    
                                    <p>Dear {dto.FirstName} {dto.LastName},</p>
                                    
                                    <p>Your account has been created successfully. You have been assigned the role of <strong>{roleName}</strong> at {serviceCenterName}.</p>
                                    
                                    <div style='background-color: #f0f7ff; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #2563eb;'>
                                        <h3 style='margin-top: 0; color: #2563eb;'>Your Login Credentials</h3>
                                        <p style='margin: 10px 0;'><strong>Email:</strong> {dto.Email}</p>
                                        <p style='margin: 10px 0;'><strong>Password:</strong> {plainPassword}</p>
                                        <p style='margin: 10px 0;'><strong>Role:</strong> {roleName}</p>
                                    </div>
                                    
                                    <div style='background-color: #fff3cd; padding: 15px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ffc107;'>
                                        <p style='margin: 0;'><strong>⚠️ Important Security Note:</strong></p>
                                        <p style='margin: 10px 0 0 0;'>For security reasons, please change your password after your first login. Keep your credentials secure and do not share them with anyone.</p>
                                    </div>
                                    
                                    <p>If you have any questions or need assistance, please contact your Service Center Administrator.</p>
                                    
                                    <p style='margin-top: 30px;'>Best regards,<br/>
                                    <strong>{serviceCenterName} Team</strong></p>
                                </div>
                                
                                <div style='text-align: center; margin-top: 20px; color: #666; font-size: 12px;'>
                                    <p>This is an automated message. Please do not reply to this email.</p>
                                </div>
                            </div>
                        </body>
                        </html>
                    ";

                    await _emailService.SendEmailAsync(dto.Email, emailSubject, emailBody);
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the user creation
                    Console.WriteLine($"Failed to send welcome email to {dto.Email}: {ex.Message}");
                    // User is still created successfully even if email fails
                }
            }

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

        // Get users for a specific service center (Cashiers and Data Operators only)
        [HttpGet("service-center/{serviceCenterId}/users")]
        public async Task<IActionResult> GetServiceCenterUsers(int serviceCenterId)
        {
            // Verify service center exists
            var serviceCenter = await _context.ServiceCenters.FindAsync(serviceCenterId);
            if (serviceCenter == null)
                return NotFound($"Service center with ID {serviceCenterId} not found.");

            // Get users assigned to this service center with roles 4 (Cashier) or 5 (DataOperator)
            var users = await _context.Users
                .Include(u => u.UserRole)
                .Where(u => u.Station_id == serviceCenterId && (u.UserRoleId == 4 || u.UserRoleId == 5))
                .Select(u => new
                {
                    id = u.UserId,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    userRole = u.UserRole.UserRoleName,
                    station_id = u.Station_id
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRole)
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    id = u.UserId,
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    email = u.Email,
                    userRole = u.UserRole.UserRoleName,
                    station_id = u.Station_id
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("User not found");

            return Ok(user);
        }

    }
}
