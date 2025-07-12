using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;
using Vpassbackend.DTOs;
using Vpassbackend.Models;

namespace Vpassbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceRemindersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceRemindersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ServiceReminders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceReminderDTO>>> GetServiceReminders()
        {
            var reminders = await _context.ServiceReminders
                .Include(sr => sr.Service)
                .Include(sr => sr.Vehicle)
                .Select(sr => new ServiceReminderDTO
                {
                    ServiceReminderId = sr.ServiceReminderId,
                    VehicleId = sr.VehicleId,
                    ReminderDate = sr.ReminderDate,
                    IntervalMonths = sr.IntervalMonths,
                    NotifyBeforeDays = sr.NotifyBeforeDays,
                    Notes = sr.Notes,
                    IsActive = sr.IsActive,
                    CreatedAt = sr.CreatedAt,
                    UpdatedAt = sr.UpdatedAt,
                    ServiceName = sr.Service.ServiceName,
                    VehicleRegistrationNumber = sr.Vehicle.RegistrationNumber,
                    VehicleBrand = sr.Vehicle.Brand,
                    VehicleModel = sr.Vehicle.Model
                })
                .ToListAsync();

            return Ok(reminders);
        }

        // GET: api/ServiceReminders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceReminderDTO>> GetServiceReminder(int id)
        {
            var serviceReminder = await _context.ServiceReminders
                .Include(sr => sr.Service)
                .Include(sr => sr.Vehicle)
                .FirstOrDefaultAsync(sr => sr.ServiceReminderId == id);

            if (serviceReminder == null)
            {
                return NotFound();
            }

            var reminderDto = new ServiceReminderDTO
            {
                ServiceReminderId = serviceReminder.ServiceReminderId,
                VehicleId = serviceReminder.VehicleId,
                ReminderDate = serviceReminder.ReminderDate,
                IntervalMonths = serviceReminder.IntervalMonths,
                NotifyBeforeDays = serviceReminder.NotifyBeforeDays,
                Notes = serviceReminder.Notes,
                IsActive = serviceReminder.IsActive,
                CreatedAt = serviceReminder.CreatedAt,
                UpdatedAt = serviceReminder.UpdatedAt,
                ServiceName = serviceReminder.Service.ServiceName,
                VehicleRegistrationNumber = serviceReminder.Vehicle.RegistrationNumber,
                VehicleBrand = serviceReminder.Vehicle.Brand,
                VehicleModel = serviceReminder.Vehicle.Model
            };

            return Ok(reminderDto);
        }

        // GET: api/ServiceReminders/Vehicle/5
        [HttpGet("Vehicle/{vehicleId}")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous] // Allow without authentication for testing
        public async Task<ActionResult<IEnumerable<ServiceReminderDTO>>> GetVehicleServiceReminders(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);

            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            var reminders = await _context.ServiceReminders
                .Include(sr => sr.Service)
                .Include(sr => sr.Vehicle)
                .Where(sr => sr.VehicleId == vehicleId)
                .Select(sr => new ServiceReminderDTO
                {
                    ServiceReminderId = sr.ServiceReminderId,
                    VehicleId = sr.VehicleId,
                    ReminderDate = sr.ReminderDate,
                    IntervalMonths = sr.IntervalMonths,
                    NotifyBeforeDays = sr.NotifyBeforeDays,
                    Notes = sr.Notes,
                    IsActive = sr.IsActive,
                    CreatedAt = sr.CreatedAt,
                    UpdatedAt = sr.UpdatedAt,
                    ServiceName = sr.Service.ServiceName,
                    VehicleRegistrationNumber = sr.Vehicle.RegistrationNumber,
                    VehicleBrand = sr.Vehicle.Brand,
                    VehicleModel = sr.Vehicle.Model
                })
                .ToListAsync();

            return Ok(reminders);
        }

        // GET: api/ServiceReminders/Upcoming
        [HttpGet("Upcoming")]
        public async Task<ActionResult<IEnumerable<ServiceReminderDTO>>> GetUpcomingReminders(int? days = 30)
        {
            if (!days.HasValue || days < 1)
            {
                days = 30; // Default to 30 days if not specified or invalid
            }

            var today = DateTime.UtcNow.Date;
            var cutoffDate = today.AddDays(days.Value);

            var upcomingReminders = await _context.ServiceReminders
                .Include(sr => sr.Service)
                .Include(sr => sr.Vehicle)
                .Where(sr => sr.IsActive && sr.ReminderDate <= cutoffDate && sr.ReminderDate >= today)
                .OrderBy(sr => sr.ReminderDate)
                .Select(sr => new ServiceReminderDTO
                {
                    ServiceReminderId = sr.ServiceReminderId,
                    VehicleId = sr.VehicleId,
                    ReminderDate = sr.ReminderDate,
                    IntervalMonths = sr.IntervalMonths,
                    NotifyBeforeDays = sr.NotifyBeforeDays,
                    Notes = sr.Notes,
                    IsActive = sr.IsActive,
                    CreatedAt = sr.CreatedAt,
                    UpdatedAt = sr.UpdatedAt,
                    ServiceName = sr.Service.ServiceName,
                    VehicleRegistrationNumber = sr.Vehicle.RegistrationNumber,
                    VehicleBrand = sr.Vehicle.Brand,
                    VehicleModel = sr.Vehicle.Model
                })
                .ToListAsync();

            return Ok(upcomingReminders);
        }

        // POST: api/ServiceReminders
        [HttpPost]
        public async Task<ActionResult<ServiceReminderDTO>> CreateServiceReminder(CreateServiceReminderDTO createDto)
        {
            // Validate the vehicle exists
            var vehicle = await _context.Vehicles.FindAsync(createDto.VehicleId);
            if (vehicle == null)
            {
                return NotFound("Vehicle not found");
            }

            // Create the new reminder with default service (you might want to adjust this logic)
            var serviceReminder = new ServiceReminder
            {
                VehicleId = createDto.VehicleId,
                ServiceId = 1, // Using a default service ID - you may need to adjust this
                ReminderDate = createDto.ReminderDate,
                IntervalMonths = createDto.IntervalMonths,
                NotifyBeforeDays = createDto.NotifyBeforeDays,
                Notes = createDto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceReminders.Add(serviceReminder);
            await _context.SaveChangesAsync();

            // Load related entities for the response
            await _context.Entry(serviceReminder).Reference(sr => sr.Service).LoadAsync();
            await _context.Entry(serviceReminder).Reference(sr => sr.Vehicle).LoadAsync();

            var reminderDto = new ServiceReminderDTO
            {
                ServiceReminderId = serviceReminder.ServiceReminderId,
                VehicleId = serviceReminder.VehicleId,
                ReminderDate = serviceReminder.ReminderDate,
                IntervalMonths = serviceReminder.IntervalMonths,
                NotifyBeforeDays = serviceReminder.NotifyBeforeDays,
                Notes = serviceReminder.Notes,
                IsActive = serviceReminder.IsActive,
                CreatedAt = serviceReminder.CreatedAt,
                UpdatedAt = serviceReminder.UpdatedAt,
                ServiceName = serviceReminder.Service.ServiceName,
                VehicleRegistrationNumber = serviceReminder.Vehicle.RegistrationNumber,
                VehicleBrand = serviceReminder.Vehicle.Brand,
                VehicleModel = serviceReminder.Vehicle.Model
            };

            return CreatedAtAction(nameof(GetServiceReminder), new { id = serviceReminder.ServiceReminderId }, reminderDto);
        }

        // PUT: api/ServiceReminders/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateServiceReminder(int id, UpdateServiceReminderDTO updateDto)
        {
            var serviceReminder = await _context.ServiceReminders.FindAsync(id);
            if (serviceReminder == null)
            {
                return NotFound();
            }

            // Update the reminder
            serviceReminder.ReminderDate = updateDto.ReminderDate;
            serviceReminder.IntervalMonths = updateDto.IntervalMonths;
            serviceReminder.NotifyBeforeDays = updateDto.NotifyBeforeDays;
            serviceReminder.Notes = updateDto.Notes;
            serviceReminder.IsActive = updateDto.IsActive;
            serviceReminder.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceReminderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ServiceReminders/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteServiceReminder(int id)
        {
            var serviceReminder = await _context.ServiceReminders.FindAsync(id);
            if (serviceReminder == null)
            {
                return NotFound();
            }

            _context.ServiceReminders.Remove(serviceReminder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ServiceReminderExists(int id)
        {
            return _context.ServiceReminders.Any(e => e.ServiceReminderId == id);
        }
    }
}
