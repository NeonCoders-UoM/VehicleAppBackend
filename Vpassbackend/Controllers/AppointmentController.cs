using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(AppointmentCreateDTO dto)
        {
            try
            {
                var result = await _service.CreateAppointmentAsync(dto);
                return Created("", result);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Get all appointments for a customer’s vehicle
        [Authorize]
        [HttpGet("customer/{customerId}/vehicle/{vehicleId}")]
        public async Task<IActionResult> GetAppointmentsByVehicle(int customerId, int vehicleId)
        {
            var result = await _service.GetAppointmentsByCustomerVehicleAsync(customerId, vehicleId);
            return Ok(result);
        }

        // Get customer appointment detail
        [Authorize]
        [HttpGet("customer/details/{appointmentId}")]
        public async Task<IActionResult> GetCustomerAppointmentDetail(int appointmentId)
        {
            var result = await _service.GetCustomerAppointmentDetailsAsync(appointmentId);
            return Ok(result);
        }

        // Get all appointments for a service center
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin,Cashier")]
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetAppointmentsForStation(int stationId)
        {
            var result = await _service.GetAppointmentsForServiceCenterAsync(stationId);
            return Ok(result);
        }

        // View full appointment details
        [Authorize(Roles = "SuperAdmin,Admin,ServiceCenterAdmin,Cashier")]
        [HttpGet("station/details/{appointmentId}")]
        public async Task<IActionResult> GetAppointmentDetailForAdmin(int appointmentId)
        {
            var result = await _service.GetAppointmentDetailForAdminAsync(appointmentId);
            return Ok(result);
        }
    }
}
