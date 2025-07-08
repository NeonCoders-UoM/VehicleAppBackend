using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByCustomerId(int customerId)
        {
            var appointments = await _appointmentService.GetAppointmentsByCustomerIdAsync(customerId);
            return Ok(appointments);
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByVehicleId(int vehicleId)
        {
            var appointments = await _appointmentService.GetAppointmentsByVehicleIdAsync(vehicleId);
            return Ok(appointments);
        }

        [HttpGet("service/{serviceId}")]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointmentsByServiceId(int serviceId)
        {
            var appointments = await _appointmentService.GetAppointmentsByServiceIdAsync(serviceId);
            return Ok(appointments);
        }

        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> CreateAppointment(AppointmentCreateDto appointmentCreateDto)
        {
            var appointment = await _appointmentService.CreateAppointmentAsync(appointmentCreateDto);
            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.AppointmentId }, appointment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AppointmentDto>> UpdateAppointment(int id, AppointmentUpdateDto appointmentUpdateDto)
        {
            var appointment = await _appointmentService.UpdateAppointmentAsync(id, appointmentUpdateDto);
            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAppointment(int id)
        {
            var result = await _appointmentService.DeleteAppointmentAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
