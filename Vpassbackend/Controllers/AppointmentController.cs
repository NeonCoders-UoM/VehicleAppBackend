using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppointmentService _service;
        public AppointmentController(AppointmentService service) => _service = service;

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

        // Cleanup duplicate appointments - keep only the latest one for each customer/vehicle/date
        [HttpDelete("cleanup-duplicates")]
        public async Task<IActionResult> CleanupDuplicateAppointments()
        {
            try
            {
                var result = await _service.CleanupDuplicateAppointmentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during cleanup", error = ex.Message });
            }
        }

        // Get appointment statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetAppointmentStatistics()
        {
            try
            {
                var result = await _service.GetAppointmentStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting statistics", error = ex.Message });
            }
        }

        // Calculate service cost without creating appointment
        [HttpPost("calculate-cost")]
        public async Task<IActionResult> CalculateServiceCost([FromBody] AppointmentCreateDTO dto)
        {
            try
            {
                var cost = await _service.CalculateServiceCostAsync(dto.Station_id, dto.ServiceIds);
                return Ok(new { totalCost = cost });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while calculating cost", error = ex.Message });
            }
        }

        // Create confirmed appointment (for final booking)
        [HttpPost("create-confirmed")]
        public async Task<IActionResult> CreateConfirmed([FromBody] AppointmentCreateDTO dto)
        {
            try
            {
                var result = await _service.CreateConfirmedAppointmentAsync(dto);
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

        // Complete an appointment
        [HttpPost("{appointmentId}/complete")]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            try
            {
                var result = await _service.CompleteAppointmentAsync(appointmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (InvalidOperationException invEx)
            {
                return BadRequest(new { message = invEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while completing the appointment", error = ex.Message });
            }
        }

        // Add services to an existing appointment
        [HttpPost("{appointmentId}/add-services")]
        public async Task<IActionResult> AddServicesToAppointment(int appointmentId, [FromBody] List<string> serviceNames)
        {
            try
            {
                var result = await _service.AddServicesToAppointmentAsync(appointmentId, serviceNames);
                return Ok(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (InvalidOperationException invEx)
            {
                return BadRequest(new { message = invEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding services", error = ex.Message });
            }
        }

        // Get loyalty points for a specific appointment
        [HttpGet("{appointmentId}/loyalty-points")]
        public async Task<IActionResult> GetLoyaltyPointsForAppointment(int appointmentId)
        {
            try
            {
                var result = await _service.GetLoyaltyPointsForAppointmentAsync(appointmentId);
                return Ok(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching loyalty points", error = ex.Message });
            }
        }

        // Apply loyalty discount to appointment
        [HttpPost("{appointmentId}/apply-loyalty-discount")]
        public async Task<IActionResult> ApplyLoyaltyDiscount(int appointmentId, [FromBody] int pointsToRedeem)
        {
            try
            {
                var result = await _service.ApplyLoyaltyDiscountAsync(appointmentId, pointsToRedeem);
                return Ok(result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (InvalidOperationException invEx)
            {
                return BadRequest(new { message = invEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while applying loyalty discount", error = ex.Message });
            }
        }
    }
}
