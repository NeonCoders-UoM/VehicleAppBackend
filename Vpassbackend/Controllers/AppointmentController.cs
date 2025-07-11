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

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetAllForCustomer(int customerId)
        {
            var result = await _service.GetCustomerAppointmentsAsync(customerId);
            return Ok(result);
        }

        [HttpGet("customer/details/{appointmentId}")]
        public async Task<IActionResult> GetCustomerAppointmentDetail(int appointmentId)
        {
            var result = await _service.GetCustomerAppointmentDetailsAsync(appointmentId);
            return Ok(result);
        }

        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetAllForStation(int stationId)
        {
            var result = await _service.GetAppointmentsForServiceCenterAsync(stationId);
            return Ok(result);
        }

        [HttpGet("station/details/{appointmentId}")]
        public async Task<IActionResult> GetAdminAppointmentDetail(int appointmentId)
        {
            var result = await _service.GetAppointmentDetailForAdminAsync(appointmentId);
            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, AppointmentStatusUpdateDTO dto)
        {
            await _service.UpdateAppointmentStatusAsync(id, dto.Status);
            return NoContent();
        }
    }
}
