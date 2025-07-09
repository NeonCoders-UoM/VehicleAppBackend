using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppointmentBookingService _appointmentService;

        public AppointmentsController(AppointmentBookingService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: api/Appointments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentConfirmationDTO>> GetAppointment(int id)
        {
            try
            {
                var appointmentDetails = await _appointmentService.GetAppointmentDetails(id);
                return appointmentDetails;
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // POST: api/Appointments/NearbyServiceCenters
        [HttpPost("NearbyServiceCenters")]
        public async Task<ActionResult<List<ServiceCenterRecommendationDTO>>> GetNearbyServiceCenters(NearbyServiceCentersRequestDTO request)
        {
            try
            {
                var serviceCenters = await _appointmentService.GetNearbyServiceCenters(
                    request.Latitude,
                    request.Longitude,
                    request.ServiceIds,
                    request.AppointmentDate
                );

                return serviceCenters;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Appointments/Book
        [HttpPost("Book")]
        [Authorize] // Ensure the user is authenticated
        public async Task<ActionResult<AppointmentConfirmationDTO>> BookAppointment(AppointmentBookingDTO booking)
        {
            try
            {
                var confirmation = await _appointmentService.BookAppointment(booking);
                return CreatedAtAction(nameof(GetAppointment), new { id = confirmation.AppointmentId }, confirmation);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Appointments/ProcessPayment
        [HttpPost("ProcessPayment")]
        [Authorize] // Ensure the user is authenticated
        public async Task<ActionResult<AppointmentConfirmationDTO>> ProcessPayment(AppointmentPaymentDTO payment)
        {
            try
            {
                var confirmation = await _appointmentService.ProcessPayment(payment);
                return confirmation;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
