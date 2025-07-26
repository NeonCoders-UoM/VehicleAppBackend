using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;
using Microsoft.EntityFrameworkCore;
using Vpassbackend.Data;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _service;
        private readonly ApplicationDbContext _context;
        
        public AppointmentController(AppointmentService service, ApplicationDbContext context) 
        { 
            _service = service; 
            _context = context;
        }

        // Customer creates a new appointment
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AppointmentCreateDTO dto)
        {
            try
            {
                var result = await _service.CreateAppointmentAsync(dto);
                return Ok(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Customer Get all appointments by their vehicle
        [HttpGet("customer/{customerId}/vehicle/{vehicleId}")]
        public async Task<IActionResult> GetAppointmentsByVehicle(int customerId, int vehicleId)
        {
            var result = await _service.GetAppointmentsByCustomerVehicleAsync(customerId, vehicleId);
            return Ok(result);
        }

        // Customer Get full appointment details
        [HttpGet("customer/{customerId}/vehicle/{vehicleId}/details/{appointmentId}")]
        public async Task<IActionResult> GetCustomerAppointmentDetail(int customerId, int vehicleId, int appointmentId)
        {
            try
            {
                var result = await _service.GetCustomerAppointmentDetailsAsync(customerId, vehicleId, appointmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }

        // Admin  Get list of appointments for a service center
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetAppointmentsForStation(int stationId)
        {
            var result = await _service.GetAppointmentsForServiceCenterAsync(stationId);
            return Ok(result);
        }

        // Admin  Get full appointment detail
        [HttpGet("station/{stationId}/details/{appointmentId}")]
        public async Task<IActionResult> GetAppointmentDetailForAdmin(int stationId, int appointmentId)
        {
            try
            {
                var result = await _service.GetAppointmentDetailForAdminAsync(stationId, appointmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }

        // Admin View customer vehicle appointment details
        [HttpGet("station/{stationId}/customer/{customerId}/vehicle/{vehicleId}/details/{appointmentId}")]
        public async Task<IActionResult> GetAdminAppointmentVehicleDetail(int stationId, int customerId, int vehicleId, int appointmentId)
        {
            try
            {
                var result = await _service.GetAdminAppointmentDetailsByVehicleAsync(
                    stationId, customerId, vehicleId, appointmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException knf)
            {
                return NotFound(new { message = knf.Message });
            }
        }

        // POST: api/Appointment/{appointmentId}/complete
        [HttpPost("{appointmentId}/complete")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            var result = await _service.CompleteAppointmentAsync(appointmentId);
            if (!result)
                return NotFound(new { message = "Appointment not found." });
            return Ok(new { message = "Appointment marked as completed and notification sent." });
        }

        // GET: api/Appointment/{appointmentId}/loyalty-points
        [HttpGet("{appointmentId}/loyalty-points")]
        public async Task<IActionResult> GetAppointmentLoyaltyPoints(int appointmentId)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.AppointmentServices)
                    .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

                if (appointment == null)
                    return NotFound(new { message = "Appointment not found." });

                // Get service IDs from the appointment
                var serviceIds = appointment.AppointmentServices.Select(s => s.ServiceId).ToList();

                // Get loyalty points from service center services
                var serviceCenterServices = await _context.ServiceCenterServices
                    .Where(scs => scs.Station_id == appointment.Station_id && serviceIds.Contains(scs.ServiceId))
                    .ToListAsync();

                int totalLoyaltyPoints = serviceCenterServices.Sum(scs => scs.LoyaltyPoints ?? 0);

                return Ok(new
                {
                    appointmentId = appointmentId,
                    customerId = appointment.CustomerId,
                    vehicleId = appointment.VehicleId,
                    stationId = appointment.Station_id,
                    loyaltyPoints = totalLoyaltyPoints,
                    services = serviceCenterServices.Select(scs => new
                    {
                        serviceId = scs.ServiceId,
                        loyaltyPoints = scs.LoyaltyPoints ?? 0
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving appointment loyalty points", error = ex.Message });
            }
        }
    }
}
