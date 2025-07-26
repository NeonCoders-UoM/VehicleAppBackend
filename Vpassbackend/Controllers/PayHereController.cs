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
        private const string MerchantId = "1230582"; // TODO: Move to config
        private const string MerchantSecret = "MTA4NzE3ODU2ODQwNTA4MTE1OTQzOTQxMDE0MzcyMjAyNTg2MDgy"; // TODO: Move to config
        private const string NotifyUrl = "https://428b46b34e0b.ngrok-free.app/api/payhere/notify"; // TODO: Replace with your ngrok URL
        private const string ReturnUrl = "http://localhost:8080/payment-success"; // TODO: Replace with your frontend return URL
        private const string CancelUrl = "http://localhost:8080/payment-cancel"; // TODO: Replace with your frontend cancel URL

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
                returnUrl = $"http://localhost:8080/payment_success.html?appointmentId={request.AppointmentId}&order_id={orderId}";
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
                returnUrl = $"http://localhost:8080/service-history?vehicleId={request.VehicleId}&order_id={orderId}";

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

                return Ok(new { 
                    message = $"Removed {pendingAppointments.Count} pending appointments",
                    removedCount = pendingAppointments.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error during cleanup", error = ex.Message });
            }
        }
    }
} 