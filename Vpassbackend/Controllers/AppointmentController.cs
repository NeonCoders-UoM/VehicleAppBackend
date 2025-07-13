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

        [HttpPost]
        public async Task<IActionResult> Create(AppointmentCreateDTO dto)
        {
            var result = await _service.CreateAppointmentAsync(dto);
            return Created("", result);
        }

        // Get all appointments for a customer’s vehicle
        [HttpGet("customer/{customerId}/vehicle/{vehicleId}")]
        public async Task<IActionResult> GetAppointmentsByVehicle(int customerId, int vehicleId)
        {
            var result = await _service.GetAppointmentsByCustomerVehicleAsync(customerId, vehicleId);
            return Ok(result);
        }

        // Get customer appointment detail
        [HttpGet("customer/details/{appointmentId}")]
        public async Task<IActionResult> GetCustomerAppointmentDetail(int appointmentId)
        {
            var result = await _service.GetCustomerAppointmentDetailsAsync(appointmentId);
            return Ok(result);
        }

        // Get all appointments for a service center
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetAppointmentsForStation(int stationId)
        {
            var result = await _service.GetAppointmentsForServiceCenterAsync(stationId);
            return Ok(result);
        }

        // View full appointment details
        [HttpGet("station/details/{appointmentId}")]
        public async Task<IActionResult> GetAppointmentDetailForAdmin(int appointmentId)
        {
            var result = await _service.GetAppointmentDetailForAdminAsync(appointmentId);
            return Ok(result);
        }
    }
}
