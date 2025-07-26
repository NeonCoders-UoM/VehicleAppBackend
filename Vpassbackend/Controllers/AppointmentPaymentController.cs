using Microsoft.AspNetCore.Mvc;
using Vpassbackend.DTOs;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentPaymentController : ControllerBase
    {
        private readonly AppointmentPaymentService _paymentService;

        public AppointmentPaymentController(AppointmentPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet("calculate/{appointmentId}")]
        public async Task<IActionResult> CalculateAdvancePayment(
            int appointmentId,
            [FromQuery] int customerId,
            [FromQuery] int vehicleId)
        {
            try
            {
                var calculation = await _paymentService.CalculateAdvancePaymentAsync(appointmentId, customerId, vehicleId);
                return Ok(calculation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while calculating advance payment", error = ex.Message });
            }
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] AdvancePaymentRequestDTO request)
        {
            try
            {
                var payment = await _paymentService.CreateAppointmentPaymentAsync(request);
                return Ok(payment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating payment", error = ex.Message });
            }
        }

        [HttpGet("status/{appointmentId}")]
        public async Task<IActionResult> GetPaymentStatus(int appointmentId)
        {
            try
            {
                var status = await _paymentService.GetPaymentStatusAsync(appointmentId);
                if (status == null)
                {
                    return NotFound(new { message = "Payment not found for this appointment" });
                }
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting payment status", error = ex.Message });
            }
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdatePaymentStatus([FromQuery] string orderId, [FromQuery] string status)
        {
            try
            {
                var success = await _paymentService.UpdatePaymentStatusAsync(orderId, status);
                if (!success)
                {
                    return BadRequest(new { message = "Invalid order ID or payment not found" });
                }
                return Ok(new { message = "Payment status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating payment status", error = ex.Message });
            }
        }

        // Test endpoint to verify advance payment calculation
        [HttpGet("test-calculation")]
        public async Task<IActionResult> TestAdvancePaymentCalculation([FromQuery] decimal totalCost)
        {
            try
            {
                decimal advancePaymentAmount;
                if (totalCost < 10000)
                {
                    advancePaymentAmount = Math.Round(totalCost * 0.10m, 2); // 10% of total cost
                }
                else
                {
                    advancePaymentAmount = 1000m; // Fixed Rs. 1000 for costs >= Rs. 10,000
                }

                return Ok(new
                {
                    totalCost = totalCost,
                    advancePaymentAmount = advancePaymentAmount,
                    remainingAmount = totalCost - advancePaymentAmount,
                    calculationType = totalCost < 10000 ? "10% of total cost" : "Fixed Rs. 1000"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while testing calculation", error = ex.Message });
            }
        }
    }
} 