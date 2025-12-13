using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Vpassbackend.Data;
using Vpassbackend.Models;
using System.Threading.Tasks;
using System;
using System.Linq;
using Vpassbackend.Services;

namespace Vpassbackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayHereController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string PayHereSandboxUrl = "https://sandbox.payhere.lk/pay/checkout";
        private const string MerchantId = "1233023"; // TODO: Move to config
        private const string MerchantSecret = "NjEyMTA2MDk3Mjg1MDExODAwMjc5NTUwNTk1MjEwNTg3OTg0MA=="; // TODO: Move to config
        private const string NotifyUrl = "https://d2ba38d700ef.ngrok-free.app/api/payhere/notify"; // TODO: Replace with your ngrok URL
        private const string ReturnUrl = "http://localhost:8081/payment-success"; // TODO: Replace with your frontend return URL
        private const string CancelUrl = "http://localhost:8081/payment-cancel"; // TODO: Replace with your frontend cancel URL

        private readonly AppointmentPaymentService _appointmentPaymentService;

        public PayHereController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, AppointmentPaymentService appointmentPaymentService)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _appointmentPaymentService = appointmentPaymentService;
        }

        // DTO for session creation
        public class CreateSessionRequest
        {
            public int VehicleId { get; set; }
            public string UserEmail { get; set; }
            public string UserName { get; set; }
            public int? AppointmentId { get; set; } // Optional: for appointment payments
            public decimal? Amount { get; set; } // Optional: for custom amounts
            public int CustomerId { get; set; } // Add this for appointment payments
        }

        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
        {
            // Fetch the vehicle entity from the database
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);
            if (vehicle == null)
            {
                return NotFound(new { message = "Vehicle not found" });
            }

            // Determine payment amount and order details
            decimal paymentAmount = 500m; // Default for PDF downloads
            string orderId;
            string returnUrl;
            string items = "Service History PDF";

            if (request.AppointmentId.HasValue && request.Amount.HasValue)
            {
                // Appointment payment - create invoice like PDF payments
                var invoice = new Invoice
                {
                    VehicleId = request.VehicleId,
                    Vehicle = vehicle,
                    TotalCost = request.Amount.Value,
                    InvoiceDate = DateTime.UtcNow
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                paymentAmount = request.Amount.Value;
                orderId = $"appointment_{request.AppointmentId}_{invoice.InvoiceId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                returnUrl = $"http://localhost:8081/payment_success.html?appointmentId={request.AppointmentId}&order_id={orderId}";
                items = "Appointment Advance Payment";

                // Store PaymentLog as pending for appointment payments too
                var paymentLog = new PaymentLog
                {
                    InvoiceId = invoice.InvoiceId,
                    PaymentDate = null,
                    Status = "Pending"
                };
                _context.PaymentLogs.Add(paymentLog);
                await _context.SaveChangesAsync();

                // Update appointment status to Payment_Pending
                var appointment = await _context.Appointments.FindAsync(request.AppointmentId.Value);
                if (appointment != null)
                {
                    appointment.Status = "Payment_Pending";
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // PDF download payment
                var invoice = new Invoice
                {
                    VehicleId = request.VehicleId,
                    Vehicle = vehicle,
                    TotalCost = paymentAmount,
                    InvoiceDate = DateTime.UtcNow
                };
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                orderId = $"invoice_{invoice.InvoiceId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
                returnUrl = $"http://localhost:8081/service-history?vehicleId={request.VehicleId}&order_id={orderId}";

                // Store PaymentLog as pending (only for PDF payments)
                var paymentLog = new PaymentLog
                {
                    InvoiceId = invoice.InvoiceId,
                    PaymentDate = null,
                    Status = "Pending"
                };
                _context.PaymentLogs.Add(paymentLog);
                await _context.SaveChangesAsync();
            }

            // Prepare PayHere payment object
            var amount = paymentAmount.ToString("F2");
            var currency = "LKR";
            var hash = GeneratePayHereHash(MerchantId, orderId, amount, currency, MerchantSecret);

            var paymentData = new
            {
                merchant_id = MerchantId,
                return_url = returnUrl,
                cancel_url = CancelUrl,
                notify_url = NotifyUrl,
                order_id = orderId,
                items = items,
                amount = amount,
                currency = currency,
                first_name = request.UserName,
                last_name = "",
                email = request.UserEmail,
                phone = "0771234567",
                address = "Colombo",
                city = "Colombo",
                country = "Sri Lanka",
                hash = hash
            };

            // Return PayHere payment URL and orderId to frontend
            // (PayHere expects a POST form, so frontend will need to POST to PayHere with these fields)
            return Ok(new
            {
                payHereUrl = PayHereSandboxUrl,
                paymentFields = paymentData,
                orderId = orderId
            });
        }

        // Add hash generation method for PayHere
        private string GeneratePayHereHash(string merchantId, string orderId, string amount, string currency, string merchantSecret)
        {
            // Step 1: MD5 of merchant secret, upper case
            string merchantSecretMd5 = GetMd5Hash(merchantSecret).ToUpper();

            // Step 2: Concatenate merchant_id + order_id + amount + currency + merchantSecretMd5
            string raw = merchantId + orderId + amount + currency + merchantSecretMd5;

            // Step 3: MD5 of the concatenated string, upper case
            string hash = GetMd5Hash(raw).ToUpper();

            return hash;
        }

        private string GetMd5Hash(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        // PayHere notify_url endpoint
        [HttpPost("notify")]
        public async Task<IActionResult> Notify()
        {
            Console.WriteLine("PayHere notify endpoint called");

            // PayHere sends form-urlencoded data
            var form = await Request.ReadFormAsync();
            var orderId = form["order_id"].ToString();
            var statusCode = form["status_code"].ToString();
            var paymentStatus = statusCode == "2" ? "Paid" : "Failed";
            var paidAt = DateTime.UtcNow;

            Console.WriteLine($"Received notification - orderId: {orderId}, statusCode: {statusCode}, paymentStatus: {paymentStatus}");

            // Handle different payment types
            if (orderId.StartsWith("invoice_"))
            {
                Console.WriteLine("Processing invoice payment");
                // PDF download payment
                int invoiceId = 0;
                if (int.TryParse(orderId.Split('_')[1], out invoiceId))
                {
                    var paymentLog = await _context.PaymentLogs
                        .Include(p => p.Invoice)
                        .Where(p => p.InvoiceId == invoiceId)
                        .OrderByDescending(p => p.LogId)
                        .FirstOrDefaultAsync();
                    if (paymentLog != null)
                    {
                        paymentLog.Status = paymentStatus;
                        paymentLog.PaymentDate = paidAt;
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Invoice payment status updated to {paymentStatus}");
                    }
                }
            }
            else if (orderId.StartsWith("appointment_"))
            {
                Console.WriteLine("Processing appointment payment");
                // Appointment payment - update via AppointmentPaymentService
                var success = await _appointmentPaymentService.UpdatePaymentStatusAsync(orderId, paymentStatus);
                if (!success)
                {
                    // Log error but don't fail the notification
                    Console.WriteLine($"Failed to update appointment payment status for orderId: {orderId}");
                }
                else
                {
                    Console.WriteLine($"Appointment payment status updated successfully");
                }
            }
            else
            {
                Console.WriteLine($"Unknown orderId format: {orderId}");
            }

            return Ok();
        }

        // Check payment status by orderId
        [HttpGet("payment-status")]
        public async Task<IActionResult> GetPaymentStatus([FromQuery] string orderId)
        {
            int invoiceId = 0;
            if (orderId.StartsWith("invoice_") && int.TryParse(orderId.Split('_')[1], out invoiceId))
            {
                var paymentLog = await _context.PaymentLogs
                    .Include(p => p.Invoice)
                    .Where(p => p.InvoiceId == invoiceId)
                    .OrderByDescending(p => p.LogId)
                    .FirstOrDefaultAsync();
                if (paymentLog != null)
                {
                    return Ok(new { status = paymentLog.Status });
                }
            }
            return NotFound(new { status = "Unknown" });
        }

        // Client-side payment confirmation endpoint
        // This is called by the mobile app after receiving payment success from PayHere SDK
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            try
            {
                Console.WriteLine($"📱 Client payment confirmation received");
                Console.WriteLine($"   Order ID: {request.OrderId}");
                Console.WriteLine($"   Payment No: {request.PaymentNo}");
                Console.WriteLine($"   Status Code: {request.StatusCode}");

                // Validate status code (2 = success in PayHere)
                if (request.StatusCode != 2)
                {
                    Console.WriteLine($"❌ Invalid status code: {request.StatusCode}");
                    return BadRequest(new { message = "Payment not successful", statusCode = request.StatusCode });
                }

                var paymentStatus = "Paid";
                var paidAt = DateTime.UtcNow;

                // Handle different payment types
                if (request.OrderId.StartsWith("invoice_"))
                {
                    Console.WriteLine("📄 Processing invoice payment confirmation");
                    // PDF download payment
                    int invoiceId = 0;
                    if (int.TryParse(request.OrderId.Split('_')[1], out invoiceId))
                    {
                        var paymentLog = await _context.PaymentLogs
                            .Include(p => p.Invoice)
                            .Where(p => p.InvoiceId == invoiceId)
                            .OrderByDescending(p => p.LogId)
                            .FirstOrDefaultAsync();

                        if (paymentLog != null)
                        {
                            // Check if already paid
                            if (paymentLog.Status == "Paid")
                            {
                                Console.WriteLine("✅ Payment already marked as Paid");
                                return Ok(new { message = "Payment already confirmed", status = "Paid" });
                            }

                            paymentLog.Status = paymentStatus;
                            paymentLog.PaymentDate = paidAt;
                            await _context.SaveChangesAsync();
                            Console.WriteLine($"✅ Invoice payment status updated to {paymentStatus}");

                            return Ok(new
                            {
                                message = "Payment confirmed successfully",
                                status = paymentStatus,
                                invoiceId = invoiceId
                            });
                        }
                        else
                        {
                            Console.WriteLine($"❌ Payment log not found for invoice {invoiceId}");
                            return NotFound(new { message = "Payment log not found" });
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ Invalid order ID format: {request.OrderId}");
                        return BadRequest(new { message = "Invalid order ID format" });
                    }
                }
                else if (request.OrderId.StartsWith("appointment_"))
                {
                    Console.WriteLine("📅 Processing appointment payment confirmation");
                    // Appointment payment
                    var success = await _appointmentPaymentService.UpdatePaymentStatusAsync(request.OrderId, paymentStatus);
                    if (!success)
                    {
                        Console.WriteLine($"❌ Failed to update appointment payment");
                        return StatusCode(500, new { message = "Failed to update appointment payment status" });
                    }

                    Console.WriteLine($"✅ Appointment payment confirmed successfully");
                    return Ok(new
                    {
                        message = "Appointment payment confirmed successfully",
                        status = paymentStatus
                    });
                }
                else
                {
                    Console.WriteLine($"❌ Unknown order ID format: {request.OrderId}");
                    return BadRequest(new { message = "Unknown order ID format" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error confirming payment: {ex.Message}");
                Console.WriteLine($"📚 Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error confirming payment", error = ex.Message });
            }
        }

        // Cleanup endpoint to remove pending appointments
        [HttpDelete("cleanup-pending")]
        public async Task<IActionResult> CleanupPendingAppointments()
        {
            try
            {
                var pendingAppointments = await _context.Appointments
                    .Where(a => a.Status == "Pending")
                    .ToListAsync();

                foreach (var appointment in pendingAppointments)
                {
                    // Remove associated services first
                    var services = await _context.AppointmentServices
                        .Where(asv => asv.AppointmentId == appointment.AppointmentId)
                        .ToListAsync();
                    _context.AppointmentServices.RemoveRange(services);

                    // Remove appointment
                    _context.Appointments.Remove(appointment);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = $"Removed {pendingAppointments.Count} pending appointments",
                    removedCount = pendingAppointments.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during cleanup", error = ex.Message });
            }
        }

        // DTO for payment confirmation
        public class ConfirmPaymentRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public string PaymentNo { get; set; } = string.Empty;
            public int StatusCode { get; set; }
        }
    }
}